import { useState, useEffect } from 'react';
import api from '../../services/api';
import toast from 'react-hot-toast';

export default function FailurePrediction() {
  const [vehicles, setVehicles] = useState([]);
  const [vehicleId, setVehicleId] = useState('');
  const [partType, setPartType] = useState('Brake');
  const [months, setMonths] = useState(12);
  const [lifespan, setLifespan] = useState(50000);
  const [result, setResult] = useState(null);

  useEffect(() => {
    api.get('/profile/vehicles').then(r => setVehicles(r.data)).catch(() => {});
  }, []);

  const predict = async () => {
    if (!vehicleId) { toast.error('Select vehicle'); return; }
    try {
      const { data } = await api.get(`/profile/predict-failure/${vehicleId}`, {
        params: { partType, monthsInstalled: months, avgLifespanKm: lifespan }
      });
      setResult(data);
    } catch (e) { toast.error('Prediction failed'); }
  };

  const colorClass = (prob) => {
    if (prob > 0.75) return 'bg-red-100 text-red-800 border-red-300';
    if (prob > 0.5) return 'bg-orange-100 text-orange-800 border-orange-300';
    if (prob > 0.25) return 'bg-yellow-100 text-yellow-800 border-yellow-300';
    return 'bg-green-100 text-green-800 border-green-300';
  };

  return (
    <div>
      <h1 className="text-2xl font-bold mb-2">AI Part Failure Prediction</h1>
      <p className="text-gray-600 mb-6">ML-powered prediction based on vehicle data + part wear history</p>

      <div className="card max-w-2xl space-y-4">
        <div>
          <label className="text-sm block mb-1">Vehicle</label>
          <select className="input" value={vehicleId} onChange={e => setVehicleId(e.target.value)}>
            <option value="">-- Select Vehicle --</option>
            {vehicles.map(v => <option key={v.id} value={v.id}>{v.make} {v.model} ({v.plateNo})</option>)}
          </select>
        </div>

        <div className="grid grid-cols-3 gap-3">
          <div>
            <label className="text-sm block mb-1">Part Type</label>
            <select className="input" value={partType} onChange={e => setPartType(e.target.value)}>
              <option>Brake</option>
              <option>Battery</option>
              <option>Tire</option>
              <option>Filter</option>
              <option>SparkPlug</option>
              <option>Belt</option>
            </select>
          </div>
          <div>
            <label className="text-sm block mb-1">Months Installed</label>
            <input className="input" type="number" value={months} onChange={e => setMonths(+e.target.value)} />
          </div>
          <div>
            <label className="text-sm block mb-1">Avg Lifespan (km)</label>
            <input className="input" type="number" value={lifespan} onChange={e => setLifespan(+e.target.value)} />
          </div>
        </div>

        <button className="btn-primary" onClick={predict}>Predict Failure Risk</button>

        {result && (
          <div className={`border rounded p-4 ${colorClass(result.failureProbability)}`}>
            <h3 className="font-bold text-lg mb-2">Result: {result.recommendation}</h3>
            <div className="space-y-1 text-sm">
              <p>Failure Probability: <span className="font-bold">{(result.failureProbability * 100).toFixed(1)}%</span></p>
              <div className="w-full bg-white rounded h-3 overflow-hidden">
                <div className="bg-current h-full opacity-60" style={{ width: `${result.failureProbability * 100}%` }} />
              </div>
              <p className="text-xs mt-2">{result.message}</p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
