import React from 'react';
import './App.css';
import { Route, Routes } from 'react-router-dom';
import AuthRoutes from './components/auth/AuthRoutes';
import Dashboard from './components/dashboard/Dashboard';
import LoginPage from './pages/LoginPage';
import './assets/scss/index.scss';
import { Provider } from 'react-redux';
import commonStore from './stores/commonStore';
import Devices from './components/devices/Devices';
import DevicesDetail from './components/devices/DevicesDetail';

function App() {
  return (
    <Provider store={commonStore}>
      <Routes>
        <Route path="/" element={<AuthRoutes />}>
          <Route path="dashboard" element={<Dashboard />} />

          {/* devices */}
          <Route path="devices" element={<Devices />} />
          <Route path="devices/add" element={<DevicesDetail />} />

          {/* login */}
          <Route path="login" element={<LoginPage />} />
        </Route>
      </Routes>
    </Provider>
  );
}

export default App;
