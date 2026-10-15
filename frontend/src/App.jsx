import { useCallback, useEffect, useState } from 'react'

const API_BASE = '/api/work-requests'
const statuses = ['New', 'InProgress', 'Blocked', 'Completed']
const priorities = ['Low', 'Medium', 'High']

export default function App() {
  const [items, setItems] = useState([])
  const [status, setStatus] = useState('')
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)
  const [form, setForm] = useState({ title: '', clientName: '', description: '', priority: 'Medium', status: 'New', dueDate: '' })
  const [noteDrafts, setNoteDrafts] = useState({})
  const pageSize = 10

  const load = useCallback(async () => {
    setLoading(true); setError('')
    try {
      const params = new URLSearchParams({ page, pageSize })
      if (status) params.set('status', status)
      if (search.trim()) params.set('search', search.trim())
      const response = await fetch(`${API_BASE}?${params}`)
      if (!response.ok) throw new Error(`Unable to load requests (${response.status})`)
      const data = await response.json()
      setItems(data.items); setTotal(data.totalCount)
    } catch (e) { setError(e.message || 'Something went wrong.') }
    finally { setLoading(false) }
  }, [status, search, page])

  useEffect(() => { load() }, [load])

  async function createRequest(e) {
    e.preventDefault(); setSaving(true); setError('')
    try {
      const response = await fetch(API_BASE, {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...form, dueDate: new Date(`${form.dueDate}T00:00:00Z`).toISOString() })
      })
      if (!response.ok) {
        const data = await response.json().catch(() => ({}))
        throw new Error(data?.error?.message || 'Could not create request.')
      }
      setForm({ title: '', clientName: '', description: '', priority: 'Medium', status: 'New', dueDate: '' })
      setPage(1); await load()
    } catch (e) { setError(e.message) }
    finally { setSaving(false) }
  }

  async function changeStatus(id, nextStatus) {
    setError('')
    try {
      const response = await fetch(`${API_BASE}/${id}/status`, {
        method: 'PATCH', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status: nextStatus })
      })
      if (!response.ok) throw new Error('Could not update status.')
      await load()
    } catch (e) { setError(e.message) }
  }

  async function addNote(id) {
    const text = (noteDrafts[id] || '').trim()
    if (!text) return
    setError('')
    try {
      const response = await fetch(`${API_BASE}/${id}/notes`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ text })
      })
      if (!response.ok) throw new Error('Could not add note.')
      setNoteDrafts(old => ({ ...old, [id]: '' }))
      await load()
    } catch (e) { setError(e.message) }
  }

  const totalPages = Math.max(1, Math.ceil(total / pageSize))

  return <main className="container">
    <header>
      <div><p className="eyebrow">OPERATIONS</p><h1>Work Request Tracker</h1>
        <p className="sub">Create, search and manage client work requests.</p></div>
      <span className="count">{total} requests</span>
    </header>

    <section className="panel">
      <h2>Create work request</h2>
      <form className="form-grid" onSubmit={createRequest}>
        <label>Title *<input required maxLength="150" value={form.title} onChange={e => setForm({...form, title:e.target.value})} /></label>
        <label>Client name *<input required maxLength="150" value={form.clientName} onChange={e => setForm({...form, clientName:e.target.value})} /></label>
        <label>Priority *<select value={form.priority} onChange={e => setForm({...form, priority:e.target.value})}>{priorities.map(x => <option key={x}>{x}</option>)}</select></label>
        <label>Status *<select value={form.status} onChange={e => setForm({...form, status:e.target.value})}>{statuses.map(x => <option key={x}>{x}</option>)}</select></label>
        <label>Due date *<input required type="date" value={form.dueDate} onChange={e => setForm({...form, dueDate:e.target.value})} /></label>
        <label className="wide">Description<input maxLength="2000" value={form.description} onChange={e => setForm({...form, description:e.target.value})} /></label>
        <div className="wide"><button disabled={saving}>{saving ? 'Saving…' : 'Create request'}</button></div>
      </form>
    </section>

    <section className="panel">
      <div className="toolbar"><h2>Work requests</h2>
        <input aria-label="Search title or client" placeholder="Search title or client…" value={search} onChange={e => {setSearch(e.target.value); setPage(1)}} />
        <select aria-label="Filter by status" value={status} onChange={e => {setStatus(e.target.value); setPage(1)}}>
          <option value="">All statuses</option>{statuses.map(x => <option key={x}>{x}</option>)}
        </select>
      </div>
      {error && <p className="error" role="alert">{error}</p>}
      {loading ? <p>Loading work requests…</p> : items.length === 0 ? <p className="empty">No work requests found.</p> :
        <div className="request-list">{items.map(item => <article className="request" key={item.id}>
          <div className="request-top"><div><h3>{item.title}</h3><p className="muted">{item.clientName} · Due {new Date(item.dueDate).toLocaleDateString()}</p></div>
            <span className={`priority ${item.priority.toLowerCase()}`}>{item.priority}</span></div>
          <p>{item.description || 'No description provided.'}</p>
          <div className="request-bottom"><label>Status <select value={item.status} onChange={e => changeStatus(item.id, e.target.value)}>{statuses.map(x => <option key={x}>{x}</option>)}</select></label>
            <span className="muted">Updated {new Date(item.updatedAtUtc).toLocaleString()}</span></div>
          {item.notes?.map(note => <p className="note" key={note.id}>Note: {note.text}</p>)}
          <div className="note-row"><input aria-label={`Note for ${item.title}`} placeholder="Add a note…" value={noteDrafts[item.id] || ''} onChange={e => setNoteDrafts(old => ({...old, [item.id]:e.target.value}))} />
            <button className="secondary" onClick={() => addNote(item.id)}>Add note</button></div>
        </article>)}</div>}
      <div className="pagination"><button className="secondary" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>Previous</button>
        <span>Page {page} of {totalPages}</span>
        <button className="secondary" disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>Next</button></div>
    </section>
    <footer>Work Request Tracker · Assignment implementation</footer>
  </main>
}
