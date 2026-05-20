import { Link, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../store/auth'

export default function Layout() {
  const { role, email, logout } = useAuth()
  const nav = useNavigate()
  const onLogout = () => { logout(); nav('/login') }

  const links = role === 'Admin' ? [
    ['/admin', 'Dashboard'],
    ['/admin/staff', 'Staff'],
    ['/admin/vendors', 'Vendors'],
    ['/admin/parts', 'Parts'],
    ['/admin/purchases', 'Purchases'],
    ['/admin/reports', 'Reports'],
    ['/admin/inventory', 'Inventory']
  ] : role === 'Staff' ? [
    ['/staff', 'Dashboard'],
    ['/staff/customers', 'Customers'],
    ['/staff/sell', 'Sell'],
    ['/staff/sales', 'Sales History'],
    ['/staff/reports', 'Customer Reports']
  ] : [
    ['/customer', 'Dashboard'],
    ['/customer/profile', 'Profile'],
    ['/customer/vehicles', 'My Vehicles'],
    ['/customer/appointments', 'Appointments'],
    ['/customer/part-requests', 'Part Requests'],
    ['/customer/history', 'History'],
    ['/customer/review', 'Review'],
    ['/customer/predict', 'AI Predict']
  ]

  return (
    <div className="min-h-screen flex">
      <aside className="w-60 bg-slate-900 text-white p-4 flex flex-col">
        <div className="text-xl font-bold mb-1">VPS</div>
        <div className="text-xs text-slate-400 mb-6">{role} Panel</div>
        <nav className="flex flex-col gap-1 flex-1">
          {links.map(([to, label]) => (
            <Link key={to} to={to} className="px-3 py-2 rounded hover:bg-slate-800 text-sm">{label}</Link>
          ))}
        </nav>
        <div className="text-xs text-slate-400 mb-2 truncate">{email}</div>
        <button onClick={onLogout} className="btn-danger text-sm">Logout</button>
      </aside>
      <main className="flex-1 p-6 bg-slate-50">
        <Outlet />
      </main>
    </div>
  )
}
