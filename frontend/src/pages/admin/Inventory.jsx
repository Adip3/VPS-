import { useEffect, useState } from 'react'
import api from '../../services/api'

export default function Inventory() {
  const [low, setLow] = useState([])
  useEffect(() => { api.get('/inventory/low-stock').then(r => setLow(r.data)) }, [])

  return (
    <div>
      <h1 className="text-2xl font-bold mb-5">Inventory — Low Stock</h1>
      <div className="card">
        <p className="text-sm text-slate-500 mb-3">Items with stock below 10. Background job emails admin every hour.</p>
        <table className="w-full text-sm">
          <thead className="bg-slate-100"><tr><th className="p-2 text-left">Part</th><th>SKU</th><th>Stock</th></tr></thead>
          <tbody>
            {low.map(p => <tr key={p.partId} className="border-t">
              <td className="p-2">{p.name}</td><td>{p.sku}</td>
              <td><span className="badge bg-red-100 text-red-700">{p.stock}</span></td>
            </tr>)}
            {low.length === 0 && <tr><td colSpan={3} className="p-4 text-center text-slate-500">All stocked above threshold</td></tr>}
          </tbody>
        </table>
      </div>
    </div>
  )
}
