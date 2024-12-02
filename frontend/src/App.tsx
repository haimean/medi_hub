import React from 'react';
import './App.css';
import { Route, Routes } from 'react-router-dom';
import AuthRoutes from './components/auth/AuthRoutes';
import Dashboard from './components/dashboard/Dashboard';
import LoginPage from './pages/LoginPage';
import './assets/scss/index.scss';

function App() {
  return (
    <Routes>
      <Route path="/" element={<AuthRoutes />}>
        <Route path="dashboard" element={<Dashboard />} />
        
        {/* login page */}
        <Route path="/login" element={<LoginPage />} />
      </Route>
    </Routes>
  );
}

export default App;
