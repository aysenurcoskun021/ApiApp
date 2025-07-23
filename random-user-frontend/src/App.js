// src/App.js
import React, { useState } from 'react'; // useState'i import ediyoruz
import './App.css';
import UserList from './components/UserList';
import axios from 'axios'; // Axios'u App.js'e de import ediyoruz

function App() {
  const [message, setMessage] = useState(''); // Kullanıcıya mesaj göstermek için state
  const [refreshKey, setRefreshKey] = useState(0); // UserList'i yenilemek için bir key

  const handleCreateUser = async () => {
    setMessage("Yeni kullanıcı oluşturuluyor ve kaydediliyor..."); // Yükleme mesajı
    try {
      // Backend API'nizin Create endpoint'ine GET isteği atıyoruz
      // Buradaki URL'i de kendi API'nizin doğru adresi ve portu ile güncelleyin!
      // Yani https://localhost:7100/Create
      const response = await axios.get('https://localhost:7100/Create');

      if (response.data) {
        setMessage("Yeni kullanıcı başarıyla oluşturuldu ve veritabanına kaydedildi!");
        setRefreshKey(prevKey => prevKey + 1); // UserList bileşenini yeniden render etmek için key'i artır
      } else {
        setMessage("Kullanıcı oluşturulurken bir sorun oluştu.");
      }
    } catch (error) {
      console.error("Kullanıcı oluşturulurken hata:", error);
      setMessage("Kullanıcı oluşturulurken bir hata oluştu: " + error.message);
    }
  };

  return (
    <div className="App">
      {/* <header> etiketi ve içindeki <h1> başlığı tamamen silindi */}
      {/* <header className="App-header" style={{ backgroundColor: '#282c34', padding: '20px', color: 'white', textAlign: 'center' }}>
        <h1>Random User API Projem</h1>
      </header> */}
      <main style={{ padding: '20px' }}>
        <div style={{ marginBottom: '20px', textAlign: 'center' }}>
          <button
            onClick={handleCreateUser}
            style={{
              padding: '10px 20px',
              fontSize: '16px',
              backgroundColor: '#61dafb',
              color: '#282c34',
              border: 'none',
              borderRadius: '5px',
              cursor: 'pointer'
            }}
          >
            Yeni Kullanıcı Oluştur ve Kaydet
          </button>
          {message && <p style={{ marginTop: '10px', color: message.includes("hata") ? 'red' : 'green' }}>{message}</p>}
        </div>
        <hr />
        {/* UserList bileşenine refreshKey prop'unu veriyoruz */}
        <UserList key={refreshKey} />
      </main>
    </div>
  );
}

export default App;