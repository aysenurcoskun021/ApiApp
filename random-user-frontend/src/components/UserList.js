// src/components/UserList.js
import React, { useState, useEffect } from 'react';
import axios from 'axios'; // Axios kütüphanesini import ediyoruz

function UserList() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchUsers = async () => {
      try {
        // API'nizin GetAllUsersFromDb endpoint'ine GET isteği atıyoruz
        const response = await axios.get('https://localhost:7100/GetAllUsersFromDb');
                console.log("API'den gelen ham kullanıcı verileri:", response.data); 
//eklendi
        setUsers(response.data);
      } catch (err) {
        console.error("Veri çekilirken hata oluştu:", err);
        setError("Kullanıcılar yüklenirken bir hata oluştu. Lütfen API'nizin çalıştığından ve CORS ayarlarının doğru olduğundan emin olun.");
      } finally {
        setLoading(false);
      }
    };

    fetchUsers();
  }, []);

  if (loading) {
    return <div>Kullanıcı verileri yükleniyor...</div>;
  }

  if (error) {
    return <div style={{ color: 'red', fontWeight: 'bold', textAlign: 'center' }}>Hata: {error}</div>;
  }

  if (users.length === 0) {
    return <div style={{ textAlign: 'center', padding: '20px', fontSize: '1.1em' }}>Henüz hiç kullanıcı veritabanında bulunamadı. Lütfen "Yeni Kullanıcı Oluştur" butonuna tıklayarak veri ekleyin.</div>;
  }

  return (
    <div style={{ overflowX: 'auto', margin: '20px auto', maxWidth: '100%', padding: '0 15px' }}> {/* Yatay kaydırma ve kenar boşlukları için */}
      <h3 style={{ textAlign: 'center', marginBottom: '20px', color: '#333' }}>Veritabanındaki Kullanıcılar</h3>
      <table style={{ 
        width: '100%', 
        minWidth: '1300px', /* Kalan sütunlara göre min genişliği ayarlandı */
        borderCollapse: 'collapse', 
        borderRadius: '8px', 
        overflow: 'hidden', 
        boxShadow: '0 4px 12px rgba(0,0,0,0.1)' 
      }}>
        <thead style={{ backgroundColor: '#282c34', color: 'white' }}>
          <tr>
            <th style={tableHeaderStyle}>Resim</th>
            <th style={tableHeaderStyle}>Ad Soyad</th>
            <th style={tableHeaderStyle}>E-posta</th>
            <th style={tableHeaderStyle}>Cinsiyet</th>
            <th style={tableHeaderStyle}>Yaş</th>
            <th style={tableHeaderStyle}>Bölge</th> 
            <th style={tableHeaderStyle}>Ülke</th> 
            <th style={tableHeaderStyle}>Şehir</th> 
            <th style={tableHeaderStyle}>Posta Kodu</th> 
            <th style={tableHeaderStyle}>Telefon</th>
            <th style={tableHeaderStyle}>Cep Telefonu</th>
            <th style={tableHeaderStyle}>Ehliyet</th>
            <th style={tableHeaderStyle}>Ehliyet Yaşı</th>
            <th style={tableHeaderStyle}>Emeklilik Kayıt Tarihi</th>
            <th style={tableHeaderStyle}>Emeklilik Yaşı</th>
            <th style={tableHeaderStyle}>Kullanıcı Adı</th>
            <th style={tableHeaderStyle}>Şifre</th>
            <th style={tableHeaderStyle}>ID Türü</th> 
            <th style={tableHeaderStyle}>ID Değeri</th> 
            <th style={tableHeaderStyle}>Uyruk</th> 
          </tr>
        </thead>
        <tbody>
          {users.map((user, index) => (
            <tr key={user.login?.uuid || user.email || index} 
                style={{ borderBottom: '1px solid #eee', backgroundColor: index % 2 === 0 ? '#f9f9f9' : '#ffffff' }}>
              <td style={tableCellStyle}>
                {user.picture?.thumbnail && (
                  <img 
                    src={user.picture.thumbnail} 
                    alt={`${user.name?.first} ${user.name?.last}`} 
                    style={{ width: '50px', height: '50px', borderRadius: '50%', objectFit: 'cover' }} 
                  />
                )}
              </td>
              <td style={tableCellStyle}>{user.name?.title} {user.name?.first} {user.name?.last}</td>
              <td style={tableCellStyle}>{user.email}</td>
              <td style={tableCellStyle}>{user.gender}</td>
              <td style={tableCellStyle}>{user.dob?.age}</td>  
              <td style={tableCellStyle}>{user.location?.state}</td>               
              <td style={tableCellStyle}>{user.location?.country}</td> 
              <td style={tableCellStyle}>{user.location?.city}</td> 
              <td style={tableCellStyle}>{user.location?.postcode}</td> 
              <td style={tableCellStyle}>{user.phone}</td>
              <td style={tableCellStyle}>{user.cell}</td>
              <td style={tableCellStyle}>{new Date(user.dob?.date).toLocaleDateString()}</td>
              <td style={tableCellStyle}>{user.dob?.age}</td>
              <td style={tableCellStyle}>{new Date(user.registered?.date).toLocaleDateString()}</td>
              <td style={tableCellStyle}>{user.registered?.age}</td>
              <td style={tableCellStyle}>{user.login?.username}</td>
              <td style={tableCellStyle}>{user.login?.password}</td>
              <td style={tableCellStyle}>{user.id?.name || 'N/A'}</td> 
              <td style={tableCellStyle}>{user.id?.value || 'N/A'}</td> 
              <td style={tableCellStyle}>{user.nat}</td> {/* Uyruk */}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

// Ortak stil objeleri
const tableHeaderStyle = {
  padding: '12px 15px',
  textAlign: 'left',
  fontWeight: 'bold',
  borderBottom: '1px solid #ddd',
  whiteSpace: 'nowrap' 
};

const tableCellStyle = {
  padding: '10px 15px',
  textAlign: 'left',
  borderBottom: '1px solid #eee',
  verticalAlign: 'middle', 
  whiteSpace: 'nowrap' 
};

export default UserList;