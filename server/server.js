require('dotenv').config();
const express = require('express');
const path = require('path'); //  정적 경로 처리에 필요
const bodyParser = require('body-parser');
const cors = require('cors');

const app = express();
const PORT = process.env.PORT || 4000;;
// const axios = require('axios');

const db = require('./config/game_db');

app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: false }));

/*const friendRoutes = require("./routes/friend");
app.use("/api/friends", friendRoutes);

app.listen(PORT, () => {
    console.log(`Server is running on port ${PORT}`);
  });
*/

// WebGL 정적 파일 서빙: ./public 폴더 내 index.html, Build 폴더 등
app.use(express.static(path.join(__dirname, 'public')));

// 루트 경로 접속 시 index.html을 반환
app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

//테스트용 API
app.get('/', (req, res) => {
  res.send('서버 작동 중!');
});

app.listen(PORT, () => {
  console.log(`서버 실행됨: http://localhost:${PORT}`);
});
