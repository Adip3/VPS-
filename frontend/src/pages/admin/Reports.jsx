import { useEffect, useState } from 'react'
import api from '../../services/api'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts'

export default function Reports() {
  const [period, setPeriod] = useState('monthly')
  const [data, setData] = useState(null)
  const [top, setTop] = useState([])

  const load = () => {
    api.get(`/reports/financial?period=${period}`).then(r => setData(r.data))
    api.get('/reports/customers?type=top').then(r => setTop(r.data))
  }
  useEffect(() => { load() }, [period])

  const chartData = data ? [
    { name: 'Sales', amount: data.totalSales },
    { name: 'Purchases', amount: data.totalPurchases },
    { name: 'Profit', amount: data.profit }
  ] : []

  return (
    <div>
      <h1 className="text-2xl font-bold mb-5">Financial Reports</h1>
      <div className="flex gap-2 mb-5">
        {['daily', 'monthly', 'yearly'].map(p =>
          <button key={p} className={p === period ? 'btn-primary' : 'btn-ghost'} onClick={() => setPeriod(p)}>{p}</button>)}
      </div>
      {data && <>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-5">
          <div className="card"><div className="text-sm text-slate-500">Sales</div><div className="text-xl font-bold text-green-600">NPR {data.totalSales}</div></div>
          <div className="card"><div className="text-sm text-slate-500">Purchases</div><div className="text-xl font-bold text-blue-600">NPR {data.totalPurchases}</div></div>
          <div className="card"><div className="text-sm text-slate-500">Profit</div><div className="text-xl font-bold text-indigo-600">NPR {data.profit}</div></div>
          <div className="card"><div className="text-sm text-slate-500">Invoices</div><div className="text-xl font-bold">{data.invoiceCount}</div></div>
        </div>
        <div className="card mb-5">
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="name" /><YAxis /><Tooltip />
              <Bar dataKey="amount" fill="#4f46e5" />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </>}

      <div className="card">
        <h2 className="font-semibold mb-3">Top Spending Customers</h2>
        <table className="w-full text-sm">
          <thead className="bg-slate-100"><tr><th className="p-2 text-left">Name</th><th>Phone</th><th>Visits</th><th>Total Spent</th><th>Outstanding</th></tr></thead>
          <tbody>
            {top.map(c => <tr key={c.customerId} className="border-t">
              <td className="p-2">{c.fullName}</td><td>{c.phone}</td>
              <td>{c.visitCount}</td><td>NPR {c.totalSpent}</td><td>NPR {c.creditOutstanding}</td>
            </tr>)}
          </tbody>
        </table>
      </div>
    </div>
  )
}
