import { useEffect, useState } from 'react'
import api from '../../services/api'
import toast from 'react-hot-toast'

export default function Sell() {
  const [parts, setParts] = useState([])
  const [q, setQ] = useState('')
  const [customers, setCustomers] = useState([])
  const [customer, setCustomer] = useState(null)
  const [vehicleId, setVehicleId] = useState('')
  const [items, setItems] = useState([])
  const [paymentStatus, setPaymentStatus] = useState('Paid')

  useEffect(() => { api.get('/parts').then(r => setParts(r.data)) }, [])

  const searchCustomer = async () => {
    const { data } = await api.get(`/customers/search?q=${encodeURIComponent(q)}`)
    setCustomers(data)
  }
  const pickCustomer = (c) => {
    setCustomer(c); setCustomers([]); setQ(c.fullName)
    if (c.vehicles.length > 0) setVehicleId(c.vehicles[0].id)
  }

  const addPart = (p) => {
    if (items.find(i => i.partId === p.id)) return toast('Already added')
    setItems([...items, { partId: p.id, name: p.name, qty: 1, price: p.sellPrice }])
  }
  const updQty = (id, q) => setItems(items.map(i => i.partId === id ? { ...i, qty: +q } : i))
  const remove = (id) => setItems(items.filter(i => i.partId !== id))

  const subtotal = items.reduce((s, i) => s + i.qty * i.price, 0)
  const discount = subtotal > 5000 ? subtotal * 0.10 : 0
  const total = subtotal - discount

  const submit = async () => {
    if (!customer) return toast.error('Pick customer')
    if (items.length === 0) return toast.error('Add items')
    try {
      const { data } = await api.post('/sales-invoices', {
        customerId: customer.id,
        vehicleId: vehicleId ? +vehicleId : null,
        paymentStatus,
        items: items.map(i => ({ partId: i.partId, qty: i.qty }))
      })
      toast.success(`Invoice #${data.id} saved (NPR ${data.total})`)
      // auto-email
      try {
        await api.post(`/sales-invoices/${data.id}/email`)
        toast.success('Invoice emailed to customer')
      } catch { toast('Email skipped (SMTP off)', { icon: 'ℹ️' }) }
      setItems([]); setCustomer(null); setQ(''); setVehicleId(''); setPaymentStatus('Paid')
    } catch (err) { toast.error(err.response?.data?.error || 'Sale failed') }
  }

  return (
    <div>
      <h1 className="text-2xl font-bold mb-5">Point of Sale</h1>
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
        <div className="lg:col-span-2 card">
          <h2 className="font-semibold mb-3">Available Parts</h2>
          <div className="overflow-y-auto max-h-96">
            <table className="w-full text-sm">
              <thead className="bg-slate-100 sticky top-0"><tr><th className="p-2 text-left">Name</th><th>SKU</th><th>Stock</th><th>Price</th><th></th></tr></thead>
              <tbody>
                {parts.map(p => <tr key={p.id} className="border-t">
                  <td className="p-2">{p.name}</td><td>{p.sku}</td>
                  <td>{p.stock}</td><td>NPR {p.sellPrice}</td>
                  <td><button className="text-indigo-600 text-sm" onClick={() => addPart(p)} disabled={p.stock === 0}>Add</button></td>
                </tr>)}
              </tbody>
            </table>
          </div>
        </div>

        <div className="card">
          <h2 className="font-semibold mb-3">Cart</h2>
          <div className="mb-3">
            <label className="text-xs">Customer</label>
            <div className="flex gap-1">
              <input className="input" placeholder="Search customer" value={q} onChange={e => setQ(e.target.value)} />
              <button className="btn-ghost text-sm" onClick={searchCustomer}>Find</button>
            </div>
            {customers.length > 0 && <ul className="border rounded mt-1 max-h-40 overflow-y-auto">
              {customers.map(c => <li key={c.id} className="p-2 hover:bg-slate-100 cursor-pointer text-sm" onClick={() => pickCustomer(c)}>
                {c.fullName} <span className="text-slate-400">— {c.phone}</span>
              </li>)}
            </ul>}
            {customer && customer.vehicles.length > 0 && <select className="input mt-2" value={vehicleId} onChange={e => setVehicleId(e.target.value)}>
              {customer.vehicles.map(v => <option key={v.id} value={v.id}>{v.plateNo}</option>)}
            </select>}
          </div>

          {items.map(i => (
            <div key={i.partId} className="flex items-center gap-2 mb-2 text-sm">
              <span className="flex-1 truncate">{i.name}</span>
              <input type="number" className="input w-16" value={i.qty} onChange={e => updQty(i.partId, e.target.value)} />
              <span>NPR {i.qty * i.price}</span>
              <button className="text-red-600" onClick={() => remove(i.partId)}>×</button>
            </div>
          ))}

          <div className="border-t pt-3 mt-3 text-sm">
            <div className="flex justify-between"><span>Subtotal:</span><span>NPR {subtotal.toFixed(2)}</span></div>
            <div className="flex justify-between text-green-700"><span>Discount (10% if &gt; 5000):</span><span>NPR {discount.toFixed(2)}</span></div>
            <div className="flex justify-between font-bold text-lg mt-1"><span>Total:</span><span>NPR {total.toFixed(2)}</span></div>
          </div>

          <div className="mt-3">
            <label className="text-xs">Payment Status</label>
            <select className="input mt-1" value={paymentStatus} onChange={e => setPaymentStatus(e.target.value)}>
              <option>Paid</option><option>Credit</option><option>Partial</option>
            </select>
          </div>
          <button className="btn-primary w-full mt-4" onClick={submit}>Complete Sale & Email Invoice</button>
        </div>
      </div>
    </div>
  )
}
