import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { apiClient, getFingerprint } from '../services/api';

export default function VotePoll() {
  const { code } = useParams();
  const navigate = useNavigate();
  const [poll, setPoll] = useState(null);
  const [selectedOption, setSelectedOption] = useState(null);
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchPoll = async () => {
      try {
        const response = await apiClient.get(`/api/polls/${code}`);
        setPoll(response.data);
      } catch (err) {
        setError('Poll not found.');
      } finally {
        setIsLoading(false);
      }
    };
    fetchPoll();
  }, [code]);

  const handleVote = async () => {
    if (selectedOption === null) return;
    setIsLoading(true);
    
    try {
      const voterToken = await getFingerprint(); 
      await apiClient.post(`/api/votes/${code}`, {
        optionIndex: selectedOption,
        voterToken: voterToken
      });
      navigate(`/poll/${code}/results`);
    } catch (err) {
      if (err.response && err.response.status === 409) {
        setError('You have already voted in this poll! (Anti-cheat triggered)');
      } else {
        setError('Failed to submit vote.');
      }
      setIsLoading(false);
    }
  };

  if (isLoading) return <div style={{textAlign: 'center', marginTop: '50px'}}>Loading poll...</div>;
  if (error && !poll) return <div style={{color: 'red', textAlign: 'center', marginTop: '50px'}}>{error}</div>;

  // Biến kiểm tra xem poll đã bị đóng chưa
  const isClosed = poll.status === 'Closed';

  return (
    <div style={{ maxWidth: '500px', margin: '50px auto', padding: '20px', fontFamily: 'sans-serif', border: '1px solid #ddd', borderRadius: '8px', boxShadow: '0 4px 8px rgba(0,0,0,0.1)' }}>
      <h2 style={{ textAlign: 'center', color: '#333' }}>{poll.question}</h2>
      
      {/* THÔNG BÁO CHẶN NẾU ĐÃ ĐÓNG */}
      {isClosed && (
        <div style={{ backgroundColor: '#fff3cd', color: '#856404', padding: '15px', textAlign: 'center', borderRadius: '5px', marginBottom: '20px', fontWeight: 'bold' }}>
          This poll has been closed by the creator.
        </div>
      )}

      {error && <div style={{ color: 'red', marginBottom: '15px', textAlign: 'center', backgroundColor: '#ffe6e6', padding: '10px', borderRadius: '4px' }}>{error}</div>}
      
      <div style={{ display: 'flex', flexDirection: 'column', gap: '10px', marginBottom: '20px' }}>
        {poll.options.map((opt, index) => (
          <button
            key={index}
            onClick={() => setSelectedOption(index)}
            disabled={isClosed} // KHÓA NÚT CHỌN
            style={{
              padding: '12px',
              border: selectedOption === index ? '2px solid #007bff' : '1px solid #ccc',
              backgroundColor: selectedOption === index ? '#e6f2ff' : 'white',
              borderRadius: '5px',
              fontSize: '16px',
              transition: 'all 0.2s',
              // Mờ đi và đổi con trỏ chuột nếu bị khóa
              opacity: isClosed ? 0.6 : 1,
              cursor: isClosed ? 'not-allowed' : 'pointer' 
            }}
          >
            {opt}
          </button>
        ))}
      </div>

      <button 
        onClick={handleVote}
        disabled={isLoading || selectedOption === null || isClosed} // KHÓA NÚT SUBMIT
        style={{ width: '100%', padding: '12px', backgroundColor: '#28a745', color: 'white', border: 'none', borderRadius: '4px', fontSize: '18px', fontWeight: 'bold', cursor: (isLoading || selectedOption === null || isClosed) ? 'not-allowed' : 'pointer', opacity: (isLoading || selectedOption === null || isClosed) ? 0.6 : 1 }}
      >
        {isLoading ? 'Submitting...' : 'Submit Vote'}
      </button>
      
      <div style={{ marginTop: '20px', textAlign: 'center' }}>
        <button onClick={() => navigate(`/poll/${code}/results`)} style={{ background: 'none', border: 'none', color: '#007bff', cursor: 'pointer', textDecoration: 'underline' }}>
          Just view results
        </button>
      </div>
    </div>
  );
}