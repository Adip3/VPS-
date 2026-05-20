import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import api from '../../services/api';

export default function CustomerDashboard() {
  const [stats, setStats] = useState({ vehicles: 0, appointments: 0, partRequests: 0, history: 0 });

  useEffect(() => {
    Promise.all([
      api.get('/profile/vehicles').catch(() => ({ data: [] })),
      api.get('/profile/appointments').catch(() => ({ data: [] })),
      api.get('/profile/part-requests').catch(() => ({ data: [] })),
      api.get('/profile/history').catch(() => ({ data: [] }))
    ]).then(([v, a, pr, h]) => {
      setStats({
        vehicles: v.data.length,
        appointments: a.data.length,
        partRequests: pr.data.length,
        history: h.data.length
      });
    });
  }, []);

  const cards = [
    { title: 'My Vehicles', value: stats.vehicles, link: '/customer/vehicles', color: 'bg-blue-500' },
    { title: 'Appointments', value: stats.appointments, link: '/customer/appointments', color: 'bg-green-500' },
    { title: 'Part Requests', value: stats.partRequests, link: '/customer/part-requests', color: 'bg-purple-500' },
    { title: 'Past Purchases', value: stats.history, link: '/customer/history', color: 'bg-orange-500' }
  ];

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">Welcome Back</h1>
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
        {cards.map(c => (
          <Link key={c.title} to={c.link} className="card hover:shadow-lg transition">
            <div className={`${c.color} w-12 h-12 rounded-lg mb-3`}></div>
            <p className="text-sm text-gray-600">{c.title}</p>
            <p className="text-3xl font-bold">{c.value}</p>
          </Link>
        ))}
      </div>

      <div className="card">
        <h2 className="font-semibold mb-3">Quick Actions</h2>
        <div className="flex flex-wrap gap-2">
          <Link to="/customer/appointments" className="btn-primary">Book Appointment</Link>
          <Link to="/customer/part-requests" className="btn-ghost">Request Part</Link>
          <Link to="/customer/predict" className="btn-ghost">AI Failure Prediction</Link>
          <Link to="/customer/review" className="btn-ghost">Write Review</Link>
        </div>
      </div>
    </div>
  );
}
