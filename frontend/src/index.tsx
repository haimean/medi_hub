import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';
import reportWebVitals from './reportWebVitals';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import "ag-grid-community/styles/ag-grid.css";
import 'ag-grid-community/styles/ag-theme-alpine.css';
import 'ag-grid-enterprise';
import { LicenseManager } from 'ag-grid-enterprise';
import { BrowserRouter } from 'react-router-dom';

LicenseManager.setLicenseKey(
  'Using_this_AG_Grid_Enterprise_key_( AG-043072 )_in_excess_of_the_licence_granted_is_not_permitted___Please_report_misuse_to_( legal@ag-grid.com )___For_help_with_changing_this_key_please_contact_( info@ag-grid.com )___( Woh Hup (Private) Limited )_is_granted_a_( Single Application )_Developer_License_for_the_application_( Idd App )_only_for_( 5 )_Front-End_JavaScript_developers___All_Front-End_JavaScript_developers_working_on_( Idd App )_need_to_be_licensed___( Idd App )_has_been_granted_a_Deployment_License_Add-on_for_( 1 )_Production_Environment___This_key_works_with_AG_Grid_Enterprise_versions_released_before_( 30 June 2024 )____[v2]_MTcxOTcwMjAwMDAwMA==06dcebba0ca814180f186be750f35cab'
);

// Config react query
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
    },
  }
});

// Create DOM
const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);

root.render(
  <React.StrictMode>
    <BrowserRouter>
      <QueryClientProvider client={queryClient}>
        <App />
      </QueryClientProvider>
    </BrowserRouter>
  </React.StrictMode>

);

reportWebVitals();
