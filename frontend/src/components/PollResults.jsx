import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { Bar } from 'react-chartjs-2';
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend } from 'chart.js';
import { apiClient, createHubConnection } from '../services/api';

// Kích hoạt thư viện ChartJS
ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

export default function PollResults() {
  const { code } = useParams();
  const [poll, setPoll] = useState(null);
  const [results, setResults] = useState([]);
  const [error, setError] = useState('');

  // Kiểm tra quyền sở hữu (được lưu ở localStorage lúc Create Poll)
  const isOwner = localStorage.getItem(`poll_owner_${code}`) === 'true';

  useEffect(() => {
    // Lấy dữ liệu ban đầu
    const fetchInitialData = async () => {
      try {
        const pollRes = await apiClient.get(`/api/polls/${code}`);
        setPoll(pollRes.data);
        
        const voteRes = await apiClient.get(`/api/votes/${code}/results`);
        setResults(voteRes.data);
      } catch (err) {
        setError('Failed to load data. Is the backend running?');
      }
    };
    fetchInitialData();

    // Mở kết nối SignalR
    const connection = createHubConnection();
    connection.start()
      .then(() => {
        connection.invoke('JoinPollGroup', code);
        
        connection.on('ReceiveVoteUpdate', (updatedResults) => {
          setResults(updatedResults);
        });
      })
      .catch(err => console.error('SignalR Error: ', err));

    return () => {
      if (connection.state === 'Connected') {
        connection.invoke('LeavePollGroup', code);
        connection.stop();
      }
    };
  }, [code]);

  // Hàm xử lý khi bấm nút Đóng Poll
  const handleClosePoll = async () => {
    if (window.confirm('Are you sure you want to close this poll? No one will be able to vote anymore.')) {
      try {
        await apiClient.put(`/api/polls/${code}/close`);
        setPoll({ ...poll, status: 'Closed' }); // Ép giao diện cập nhật ngay lập tức
      } catch (err) {
        alert('Failed to close poll. It might already be closed.');
      }
    }
  };

  if (error) return <div style={{ color: 'red', textAlign: 'center', marginTop: '50px' }}>{error}</div>;
  if (!poll) return <div style={{ textAlign: 'center', marginTop: '50px' }}>Loading live results...</div>;

  // Dữ liệu vẽ biểu đồ
  const chartData = {
    labels: poll.options,
    datasets: [
      {
        label: 'Number of Votes',
        data: poll.options.map((_, index) => {
          const result = results.find(r => r.optionIndex === index);
          return result ? result.count : 0;
        }),
        backgroundColor: 'rgba(54, 162, 235, 0.7)',
        borderColor: 'rgba(54, 162, 235, 1)',
        borderWidth: 1,
        borderRadius: 4
      },
    ],
  };

  const chartOptions = {
    responsive: true,
    scales: {
      y: {
        beginAtZero: true,
        ticks: { stepSize: 1 }
      }
    },
    animation: { duration: 800 }
  };

  return (
    <div style={{ maxWidth: '700px', margin: '50px auto', padding: '20px', fontFamily: 'sans-serif' }}>
      <h2 style={{ textAlign: 'center' }}>Live Results: {poll.question}</h2>
      
      <div style={{ marginTop: '40px', padding: '20px', backgroundColor: 'white', borderRadius: '8px', boxShadow: '0 4px 12px rgba(0,0,0,0.1)' }}>
        <Bar data={chartData} options={chartOptions} />
      </div>

      <div style={{ marginTop: '30px', textAlign: 'center' }}>
        {/* Nút Close Poll (Chỉ hiện cho Owner và khi Poll chưa bị đóng) */}
        {poll.status !== 'Closed' && isOwner && (
          <button onClick={handleClosePoll} style={{ padding: '10px 20px', backgroundColor: '#dc3545', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', marginRight: '20px', fontWeight: 'bold' }}>
            Close Poll
          </button>
        )}
        
        {/* Chữ thông báo nếu Poll đã đóng */}
        {poll.status === 'Closed' && (
          <span style={{ color: 'red', fontWeight: 'bold', marginRight: '20px' }}>[ This poll is closed ]</span>
        )}
        
        <Link to={`/poll/${code}`} style={{ marginRight: '20px', textDecoration: 'none', color: '#007bff', fontWeight: 'bold' }}>← Back to Vote</Link>
        <Link to="/" style={{ textDecoration: 'none', color: '#28a745', fontWeight: 'bold' }}>Create New Poll</Link>
      </div>
    </div>
  );
}