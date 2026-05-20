import { useEffect, useState } from 'react'
import api from '../../services/api'

export default function History() {
  const [list, setList] = useState([])
  const [open, setOpen] = useState(null)

  useEffect(() => { api.get('/profile/history').then(r => setList(r.data)) }, [])

  return (
    <div>
      <h1 className="text-2xl font-bold mb-5">Purchase History</h1>
      <div className="card">
        <table className="w-full text-sm">
          <thead className="bg-slate-100"><tr><th className="p-2 text-left">Invoice</th><th>Date</th><th>Total</th><th>Status</th><th></th></tr></thead>
          <tbody>
            {list.map(s => <>
              <tr key={s.id} className="border-t">
                <td className="p-2">#{s.id}</td>
                <td>{new Date(s.date).toLocaleDateString()}</td>
                <td>NPR {s.total}</td>
                <td><span className={`badge ${s.paymentStatus === 'Paid' ? 'bg-green-100 text-green-700' : 'bg-orange-100 text-orange-700'}`}>{s.paymentStatus}</span></td>
                <td><button className="text-indigo-600 text-xs" onClick={() => setOpen(open === s.id ? null : s.id)}>Details</button></td>
              </tr>
              {open === s.id && <tr><td colSpan={5} className="bg-slate-50 p-3">
                <table className="w-full text-xs">
                  <thead><tr><th className="text-left">Part</th><th>Qty</th><th>Price</th><th>Total</th></tr></thead>
                  <tbody>{s.items.map((i, idx) => <tr key={idx}><td>{i.partName}</td><td>{i.qty}</td><td>NPR {i.unitPrice}</td><td>NPR {i.lineTotal}</td></tr>)}</tbody>
                </table>
              </td></tr>}
            </>)}
            {list.length === 0 && <tr><td colSpan={5} className="p-4 text-center text-slate-500">No history</td></tr>}
          </tbody>
        </table>
      </div>
    </div>
  )
}
