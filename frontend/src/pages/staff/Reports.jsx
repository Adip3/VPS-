import { useEffect, useState } from 'react'
import api from '../../services/api'

export default function StaffReports() {
  const [type, setType] = useState('top')
  const [data, setData] = useState([])
  useEffect(() => { api.get(`/reports/customers?type=${type}`).then(r => setData(r.data)) }, [type])

  const titles = { top: 'Top Spenders', regular: 'Regular Customers', 'credit-pending': 'Pending Credit Payments' }

  return (
    <div>
      <h1 className="text-2xl font-bold mb-5">Customer Reports</h1>
      <div className="flex gap-2 mb-5">
        {Object.keys(titles).map(t =>
          <button key={t} className={t === type ? 'btn-primary' : 'btn-ghost'} onClick={() => setType(t)}>{titles[t]}</button>)}
      </div>
      <div className="card">
        <h2 className="font-semibold mb-3">{titles[type]}</h2>
        <table className="w-full text-sm">
          <thead className="bg-slate-100"><tr><th className="p-2 text-left">Name</th><th>Phone</th><th>Visits</th><th>Total Spent</th><th>Outstanding</th></tr></thead>
          <tbody>
            {data.map(c => <tr key={c.customerId} className="border-t">
              <td className="p-2">{c.fullName}</td><td>{c.phone}</td>
              <td>{c.visitCount}</td><td>NPR {c.totalSpent}</td>
              <td className={c.creditOutstanding > 0 ? 'text-red-600 font-semibold' : ''}>NPR {c.creditOutstanding}</td>
            </tr>)}
            {data.length === 0 && <tr><td colSpan={5} className="p-4 text-center text-slate-500">No data</td></tr>}
          </tbody>
        </table>
      </div>
    </div>
  )
}
