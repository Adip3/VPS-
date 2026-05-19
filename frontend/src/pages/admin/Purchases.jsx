import { useEffect, useState } from 'react'
import api from '../../services/api'
import toast from 'react-hot-toast'

export default function Purchases() {
  const [vendors, setVendors] = useState([])
  const [parts, setParts] = useState([])
  const [items, setItems] = useState([{ partId: '', qty: 1, unitCost: 0 }])
  const [vendorId, setVendorId] = useState('')
  const [list, setList] = useState([])

  const load = () => api.get('/purchase-invoices').then(r => setList(r.data))

  useEffect(() => {
    api.get('/vendors').then(r => setVendors(r.data))
    api.get('/parts').then(r => setParts(r.data))
    load()
  }, [])

  const addRow = () => setItems([...items, { partId: '', qty: 1, unitCost: 0 }])
  const upd = (i, k, v) => setItems(items.map((it, idx) => idx === i ? { ...it, [k]: v } : it))
  const del = (i) => setItems(items.filter((_, idx) => idx !== i))

  const submit = async () => {
    try {
      await api.post('/purchase-invoices', {
        vendorId: +vendorId,
        items: items.map(i => ({ partId: +i.partId, qty: +i.qty, unitCost: +i.unitCost }))
      })
      toast.success('Purchase saved — stock updated')
      setItems([{ partId: '', qty: 1, unitCost: 0 }]); setVendorId(''); load()
    } catch (err) { toast.error(err.response?.data?.error || 'Save failed') }
  }

  const total = items.reduce((s, i) => s + (+i.qty * +i.unitCost), 0)

  return (
    <div>
      <h1 className="text-2xl font-bold mb-5">Purchase Invoices</h1>
      <div className="card mb-5">
        <div className="mb-3">
          <label className="text-xs font-medium">Vendor</label>
          <select className="input mt-1" value={vendorId} onChange={e => setVendorId(e.target.value)}>
            <option value="">-- pick vendor --</option>
            {vendors.map(v => <option key={v.id} value={v.id}>{v.name}</option>)}
          </select>
        </div>
        <table className="w-full text-sm mb-3">
          <thead className="bg-slate-100"><tr><th className="p-2 text-left">Part</th><th>Qty</th><th>Unit Cost</th><th>Total</th><th></th></tr></thead>
          <tbody>
            {items.map((it, i) => (
              <tr key={i} className="border-t">
                <td className="p-2">
                  <select className="input" value={it.partId} onChange={e => upd(i, 'partId', e.target.value)}>
                    <option value="">--</option>
                    {parts.map(p => <option key={p.id} value={p.id}>{p.name} ({p.sku})</option>)}
                  </select>
                </td>
                <td><input type="number" className="input w-20" value={it.qty} onChange={e => upd(i, 'qty', e.target.value)} /></td>
                <td><input type="number" className="input w-28" value={it.unitCost} onChange={e => upd(i, 'unitCost', e.target.value)} /></td>
                <td className="p-2">{(+it.qty * +it.unitCost).toFixed(2)}</td>
                <td><button className="text-red-600" onClick={() => del(i)}>x</button></td>
              </tr>
            ))}
          </tbody>
        </table>
        <div className="flex justify-between items-center">
          <button className="btn-ghost" onClick={addRow}>+ Add Row</button>
          <div className="text-lg font-bold">Total: NPR {total.toFixed(2)}</div>
        </div>
        <button className="btn-primary mt-4" onClick={submit}>Save Invoice</button>
      </div>

      <div className="card">
        <h2 className="font-semibold mb-3">Recent Purchases</h2>
        <table className="w-full text-sm">
          <thead className="bg-slate-100"><tr><th className="p-2 text-left">Id</th><th>Vendor</th><th>Date</th><th>Total</th></tr></thead>
          <tbody>
            {list.map(p => <tr key={p.id} className="border-t">
              <td className="p-2">#{p.id}</td><td>{p.vendorName}</td>
              <td>{new Date(p.date).toLocaleDateString()}</td><td>NPR {p.total}</td>
            </tr>)}
          </tbody>
        </table>
      </div>
    </div>
  )
}
