import { useState } from 'react';
import { apiClient } from '../services/api';

export default function CreatePoll({ onPollCreated }) {
  const [question, setQuestion] = useState('');
  const [options, setOptions] = useState(['', '']); // Start with 2 empty options
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  // Update specific option text
  const handleOptionChange = (index, value) => {
    const newOptions = [...options];
    newOptions[index] = value;
    setOptions(newOptions);
  };

  // Add new option input box (Max 6)
  const addOption = () => {
    if (options.length < 6) {
      setOptions([...options, '']);
    }
  };

  // Remove an option input box (Min 2)
  const removeOption = (indexToRemove) => {
    if (options.length > 2) {
      setOptions(options.filter((_, index) => index !== indexToRemove));
    }
  };

  // Submit data to Backend Gateway
  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    // Filter out empty options and trim whitespace
    const validOptions = options.map(opt => opt.trim()).filter(opt => opt !== '');

    if (!question.trim()) {
      setError('Question is required.');
      setIsLoading(false);
      return;
    }

    if (validOptions.length < 2) {
      setError('Please provide at least 2 valid options.');
      setIsLoading(false);
      return;
    }

    try {
      const response = await apiClient.post('/api/polls', {
        question: question.trim(),
        options: validOptions
      });

      localStorage.setItem(`poll_owner_${response.data.code}`, 'true');
      
      // Pass the new Poll Code back to the parent component
      onPollCreated(response.data.code);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to create poll. Is the backend running?');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div style={{ maxWidth: '500px', margin: '0 auto', padding: '20px', fontFamily: 'sans-serif' }}>
      <h2>Create a Real-Time Poll</h2>
      
      {error && <div style={{ color: 'red', marginBottom: '10px', padding: '10px', backgroundColor: '#ffe6e6', borderRadius: '4px' }}>{error}</div>}
      
      <form onSubmit={handleSubmit}>
        <div style={{ marginBottom: '15px' }}>
          <label style={{ display: 'block', marginBottom: '5px', fontWeight: 'bold' }}>Question:</label>
          <input
            type="text"
            value={question}
            onChange={(e) => setQuestion(e.target.value)}
            placeholder="E.g., What is your favorite programming language?"
            style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
            maxLength={500}
          />
        </div>

        <div style={{ marginBottom: '15px' }}>
          <label style={{ display: 'block', marginBottom: '5px', fontWeight: 'bold' }}>Options (2 - 6):</label>
          {options.map((opt, index) => (
            <div key={index} style={{ display: 'flex', marginBottom: '10px' }}>
              <input
                type="text"
                value={opt}
                onChange={(e) => handleOptionChange(index, e.target.value)}
                placeholder={`Option ${index + 1}`}
                style={{ flex: 1, padding: '8px', marginRight: '5px' }}
              />
              {options.length > 2 && (
                <button type="button" onClick={() => removeOption(index)} style={{ padding: '8px 12px', cursor: 'pointer', backgroundColor: '#ff4d4d', color: 'white', border: 'none', borderRadius: '4px' }}>
                  X
                </button>
              )}
            </div>
          ))}
          
          {options.length < 6 && (
            <button type="button" onClick={addOption} style={{ padding: '8px 12px', cursor: 'pointer', backgroundColor: '#e0e0e0', border: 'none', borderRadius: '4px', marginTop: '5px' }}>
              + Add Option
            </button>
          )}
        </div>

        <button 
          type="submit" 
          disabled={isLoading}
          style={{ width: '100%', padding: '10px', backgroundColor: '#007bff', color: 'white', border: 'none', borderRadius: '4px', fontSize: '16px', cursor: isLoading ? 'not-allowed' : 'pointer' }}
        >
          {isLoading ? 'Creating...' : 'Create Poll'}
        </button>
      </form>
    </div>
  );
}