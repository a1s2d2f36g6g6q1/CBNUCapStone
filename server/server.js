require('dotenv').config();
const express = require('express');
const path = require('path');
const bodyParser = require('body-parser');
const cors = require('cors');

const app = express();
const PORT = process.env.PORT || 4000;

const db = require('./config/game_db');

// 미들웨어
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: false }));

// WebGL 빌드 폴더 정적 서빙
app.use(express.static(path.join(__dirname, 'public')));

// 루트 접속 시 Unity WebGL index.html 반환
app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

// 서버 실행
app.listen(PORT, () => {
  console.log(`서버 실행됨: http://localhost:${PORT}`);
});

// friends 라우터
const friendsRouter = require('./routes/friends');
app.use('/api/friends', friendsRouter);