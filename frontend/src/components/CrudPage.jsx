import { useEffect, useState } from 'react'
import api from '../services/api'
import toast from 'react-hot-toast'

/**
 * Reusable CRUD list/form page.
 * props: title, resource (path), columns [{key,label}], formFields [{key,label,type,required}]
 */
export default function CrudPage({ title, resource, columns, formFields, idKey = 'id' }) {
  const [rows, setRows] = useState([])
  const [form, setForm] = useState({})
  const [editing, setEditing] = useState(null)

  const load = () => api.get(`/${resource}`).then(r => setRows(r.data)).catch(() => toast.error('Load failed'))
  useEffect(() => { load() }, [])

  const submit = async (e) => {
    e.preventDefault()
    try {
      if (editing) await api.put(`/${resource}/${editing}`, form)
      else await api.post(`/${resource}`, form)
      toast.success('Saved')
      setForm({}); setEditing(null); load()
    } catch (err) { toast.error(err.response?.data?.error || 'Save failed') }
  }

  const del = async (id) => {
    if (!confirm('Delete this record?')) return
    try { await api.delete(`/${resource}/${id}`); toast.success('Deleted'); load() }
    catch { toast.error('Delete failed') }
  }

  const startEdit = (r) => { setEditing(r[idKey]); setForm(r) }

  return (
    <div>
      <h1 className="text-2xl font-bold mb-5">{title}</h1>
      <div className="card mb-5">
        <h2 className="font-semibold mb-3">{editing ? 'Edit' : 'Add new'}</h2>
        <form onSubmit={submit} className="grid grid-cols-1 md:grid-cols-3 gap-3">
          {formFields.map(f => (
            <div key={f.key}>
              <label className="text-xs font-medium text-slate-600">{f.label}</label>
              <input className="input mt-1" type={f.type || 'text'} required={f.required}
                value={form[f.key] ?? ''} onChange={e => setForm({ ...form, [f.key]: f.type === 'number' ? +e.target.value : e.target.value })} />
            </div>
          ))}
          <div className="flex gap-2 items-end">
            <button className="btn-primary">Save</button>
            {editing && <button type="button" className="btn-ghost" onClick={() => { setEditing(null); setForm({}) }}>Cancel</button>}
          </div>
        </form>
      </div>

      <div className="card overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="text-left bg-slate-100">
            <tr>{columns.map(c => <th key={c.key} className="p-2">{c.label}</th>)}<th className="p-2">Actions</th></tr>
          </thead>
          <tbody>
            {rows.map(r => (
              <tr key={r[idKey]} className="border-t">
                {columns.map(c => <td key={c.key} className="p-2">{r[c.key]}</td>)}
                <td className="p-2 flex gap-2">
                  <button className="text-indigo-600 text-sm" onClick={() => startEdit(r)}>Edit</button>
                  <button className="text-red-600 text-sm" onClick={() => del(r[idKey])}>Delete</button>
                </td>
              </tr>
            ))}
            {rows.length === 0 && <tr><td colSpan={columns.length + 1} className="p-4 text-center text-slate-500">No records</td></tr>}
          </tbody>
        </table>
      </div>
    </div>
  )
}
