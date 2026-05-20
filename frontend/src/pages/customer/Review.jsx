import { useState, useEffect } from 'react';
import api from '../../services/api';
import toast from 'react-hot-toast';

export default function Review() {
  const [parts, setParts] = useState([]);
  const [reviews, setReviews] = useState([]);
  const [form, setForm] = useState({ partId: '', rating: 5, comment: '' });

  const load = async () => {
    try {
      const [pRes, rRes] = await Promise.all([
        api.get('/parts'),
        api.get('/reviews/mine')
      ]);
      setParts(pRes.data);
      setReviews(rRes.data);
    } catch (e) { /* ignore */ }
  };

  useEffect(() => { load(); }, []);

  const submit = async (e) => {
    e.preventDefault();
    if (!form.partId) { toast.error('Pick a part'); return; }
    try {
      await api.post('/reviews', { partId: +form.partId, rating: +form.rating, comment: form.comment });
      toast.success('Review submitted');
      setForm({ partId: '', rating: 5, comment: '' });
      load();
    } catch (e) { toast.error(e.response?.data?.message || 'Failed'); }
  };

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">Reviews</h1>

      <form onSubmit={submit} className="card mb-6 space-y-3 max-w-2xl">
        <h2 className="font-semibold">Write a Review</h2>
        <select className="input" value={form.partId} onChange={e => setForm({ ...form, partId: e.target.value })} required>
          <option value="">-- Select Part --</option>
          {parts.map(p => <option key={p.id} value={p.id}>{p.name} ({p.sku})</option>)}
        </select>
        <div>
          <label className="text-sm block mb-1">Rating</label>
          <div className="flex gap-2">
            {[1, 2, 3, 4, 5].map(n => (
              <button type="button" key={n} onClick={() => setForm({ ...form, rating: n })}
                className={`text-2xl ${n <= form.rating ? 'text-yellow-400' : 'text-gray-300'}`}>★</button>
            ))}
          </div>
        </div>
        <textarea className="input" rows="3" placeholder="Your comment..." value={form.comment} onChange={e => setForm({ ...form, comment: e.target.value })} />
        <button className="btn-primary" type="submit">Submit Review</button>
      </form>

      <div className="card">
        <h2 className="font-semibold mb-3">My Reviews</h2>
        {reviews.length === 0 ? (
          <p className="text-gray-500">No reviews yet</p>
        ) : (
          <div className="space-y-3">
            {reviews.map(r => (
              <div key={r.id} className="border-b pb-3 last:border-0">
                <div className="flex justify-between">
                  <span className="font-medium">{r.partName}</span>
                  <span className="text-yellow-400">{'★'.repeat(r.rating)}{'☆'.repeat(5 - r.rating)}</span>
                </div>
                <p className="text-sm text-gray-600 mt-1">{r.comment}</p>
                <p className="text-xs text-gray-400 mt-1">{new Date(r.createdAt).toLocaleDateString()}</p>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
