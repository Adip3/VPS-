import { Routes, Route, Navigate } from 'react-router-dom';
import Layout from './components/Layout';
import PrivateRoute from './components/PrivateRoute';

import Login from './pages/auth/Login';
import Register from './pages/auth/Register';

import AdminDashboard from './pages/admin/Dashboard';
import { StaffPage, VendorsPage, PartsPage } from './pages/admin/CrudPages';
import Inventory from './pages/admin/Inventory';
import Purchases from './pages/admin/Purchases';
import AdminReports from './pages/admin/Reports';

import StaffDashboard from './pages/staff/Dashboard';
import Customers from './pages/staff/Customers';
import Sell from './pages/staff/Sell';
import Sales from './pages/staff/Sales';
import StaffReports from './pages/staff/Reports';

import CustomerDashboard from './pages/customer/Dashboard';
import Profile from './pages/customer/Profile';
import Vehicles from './pages/customer/Vehicles';
import Appointments from './pages/customer/Appointments';
import PartRequests from './pages/customer/PartRequests';
import History from './pages/customer/History';
import Review from './pages/customer/Review';
import FailurePrediction from './pages/customer/FailurePrediction';

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />

      {/* Admin */}
      <Route element={<PrivateRoute roles={['Admin']} />}>
        <Route element={<Layout />}>
          <Route path="/admin" element={<AdminDashboard />} />
          <Route path="/admin/staff" element={<StaffPage />} />
          <Route path="/admin/vendors" element={<VendorsPage />} />
          <Route path="/admin/parts" element={<PartsPage />} />
          <Route path="/admin/inventory" element={<Inventory />} />
          <Route path="/admin/purchases" element={<Purchases />} />
          <Route path="/admin/reports" element={<AdminReports />} />
        </Route>
      </Route>

      {/* Staff */}
      <Route element={<PrivateRoute roles={['Staff', 'Admin']} />}>
        <Route element={<Layout />}>
          <Route path="/staff" element={<StaffDashboard />} />
          <Route path="/staff/customers" element={<Customers />} />
          <Route path="/staff/sell" element={<Sell />} />
          <Route path="/staff/sales" element={<Sales />} />
          <Route path="/staff/reports" element={<StaffReports />} />
        </Route>
      </Route>

      {/* Customer */}
      <Route element={<PrivateRoute roles={['Customer']} />}>
        <Route element={<Layout />}>
          <Route path="/customer" element={<CustomerDashboard />} />
          <Route path="/customer/profile" element={<Profile />} />
          <Route path="/customer/vehicles" element={<Vehicles />} />
          <Route path="/customer/appointments" element={<Appointments />} />
          <Route path="/customer/part-requests" element={<PartRequests />} />
          <Route path="/customer/history" element={<History />} />
          <Route path="/customer/review" element={<Review />} />
          <Route path="/customer/predict" element={<FailurePrediction />} />
        </Route>
      </Route>

      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}
