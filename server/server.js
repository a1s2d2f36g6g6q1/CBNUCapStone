require('dotenv').config();
const express = require('express');
const path = require('path');
const bodyParser = require('body-parser');
const cors = require('cors');

const app = express();
const PORT = process.env.PORT || 4000;

const db = require('./config/game_db');
const authRoutes = require("./routes/authRoutes");

// 미들웨어
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: false }));
app.use(bodyParser.json());
app.use("/api/auth", authRoutes);

// WebGL 빌드 폴더 정적 서빙
app.use(express.static(path.join(__dirname, 'public')));

// 루트 접속 시 Unity WebGL index.html 반환
app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

// POST /api/tags
app.post('/api/tags', (req, res) => {
  const { tag0, tag1, tag2, tag3 } = req.body;

  // 유효성 검사
  const tags = [tag0, tag1, tag2, tag3];
  for (let i = 0; i < tags.length; i++) {
    if (typeof tags[i] !== 'string' || tags[i].length > 20) {
      return res.status(400).json({ success: false, message: `tag${i}가 유효하지 않음` });
    }
  }

  const sql = `INSERT INTO tags_input (tag0, tag1, tag2, tag3) VALUES (?, ?, ?, ?)`;
  db.query(sql, tags, (err, result) => {
    if (err) {
      console.error('DB insert error:', err);
      return res.status(500).json({ success: false, message: 'DB 오류' });
    }
    res.json({ success: true, insertId: result.insertId });
  });
});

// 서버 실행
app.listen(PORT, () => {
  console.log(`서버 실행됨: http://localhost:${PORT}`);
});