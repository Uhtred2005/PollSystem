import { BrowserRouter as Router, Routes, Route, useNavigate } from 'react-router-dom';
import CreatePoll from './components/CreatePoll';
import VotePoll from './components/VotePoll';
import PollResults from './components/PollResults';

// Tạo một component phụ bọc form CreatePoll để xử lý chuyển trang khi tạo xong
function CreatePollWrapper() {
  const navigate = useNavigate();
  const handlePollCreated = (code) => {
    // Tự động chuyển người tạo sang trang xem Biểu đồ ngay sau khi tạo
    navigate(`/poll/${code}/results`);
  };
  
  return (
    <div>
       <header style={{ textAlign: 'center', padding: '20px', backgroundColor: '#f8f9fa', borderBottom: '1px solid #dee2e6', marginBottom: '20px', fontFamily: 'sans-serif' }}>
         <h1>Real-Time Poll System</h1>
       </header>
       <CreatePoll onPollCreated={handlePollCreated} />
    </div>
  );
}

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<CreatePollWrapper />} />
        <Route path="/poll/:code" element={<VotePoll />} />
        <Route path="/poll/:code/results" element={<PollResults />} />
      </Routes>
    </Router>
  );
}

export default App;