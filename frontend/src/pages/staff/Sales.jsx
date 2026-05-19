import { useEffect, useState } from 'react'
import api from '../../services/api'
import toast from 'react-hot-toast'

export default function Sales() {
  const [list, setList] = useState([])
  const [open, setOpen] = useState(null)
  useEffect(() => { api.get('/sales-invoices').then(r => setList(r.data)) }, [])

  const email = async (id) => {
    try { await api.post(`/sales-invoices/${id}/email`); toast.success('Resent') }
    catch { toast.error('Failed') }
  }

  return (
    <div>
      <h1 className="text-2xl font-bold mb-5">Sales History</h1>
      <div className="card">
        <table className="w-full text-sm">
          <thead className="bg-slate-100"><tr><th className="p-2 text-left">Id</th><th>Customer</th><th>Date</th><th>Subtotal</th><th>Discount</th><th>Total</th><th>Status</th><th></th></tr></thead>
          <tbody>
            {list.map(s => <>
              <tr key={s.id} className="border-t">
                <td className="p-2">#{s.id}</td><td>{s.customerName}</td>
                <td>{new Date(s.date).toLocaleDateString()}</td>
                <td>NPR {s.subTotal}</td><td>NPR {s.discount}</td><td>NPR {s.total}</td>
                <td><span className={`badge ${s.paymentStatus === 'Paid' ? 'bg-green-100 text-green-700' : 'bg-orange-100 text-orange-700'}`}>{s.paymentStatus}</span></td>
                <td>
                  <button className="text-indigo-600 text-xs" onClick={() => setOpen(open === s.id ? null : s.id)}>Items</button>
                  <button className="text-blue-600 text-xs ml-2" onClick={() => email(s.id)}>Email</button>
                </td>
              </tr>
              {open === s.id && <tr><td colSpan={8} className="bg-slate-50 p-3">
                <table className="w-full text-xs">
                  <thead><tr><th className="text-left">Part</th><th>Qty</th><th>Price</th><th>Line Total</th></tr></thead>
                  <tbody>
                    {s.items.map((i, idx) => <tr key={idx}><td>{i.partName}</td><td>{i.qty}</td><td>NPR {i.unitPrice}</td><td>NPR {i.lineTotal}</td></tr>)}
                  </tbody>
                </table>
              </td></tr>}
            </>)}
          </tbody>
        </table>
      </div>
    </div>
  )
}
