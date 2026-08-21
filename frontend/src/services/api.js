import axios from 'axios';
import * as signalR from '@microsoft/signalr';
import FingerprintJS from '@fingerprintjs/fingerprintjs';

// Define the Gateway URL (Port 5000 from Backend)
const BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

export const apiClient = axios.create({
  baseURL: BASE_URL,
});

// Get browser fingerprint for Anti-Cheat feature
export const getFingerprint = async () => {
  const fp = await FingerprintJS.load();
  const result = await fp.get();
  return result.visitorId; 
};

// Setup SignalR WebSocket connection
export const createHubConnection = () => {
  return new signalR.HubConnectionBuilder()
    .withUrl(`${BASE_URL}/hubs/poll`, {
      skipNegotiation: false,
      transport: signalR.HttpTransportType.WebSockets
    })
    .withAutomaticReconnect()
    .build();
};