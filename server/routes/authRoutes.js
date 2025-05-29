const express = require("express");
const router = express.Router();
const db = require("../config/game_db");

// 회원가입
router.post("/signup", (req, res) => {
  const { user_id, password, nickname } = req.body;

  if (!user_id || !password || !nickname)
    return res.status(400).json({ error: "필수 항목 누락" });

  const checkQuery = "SELECT * FROM users WHERE user_id = ?";
  db.query(checkQuery, [user_id], (err, results) => {
    if (err) return res.status(500).json({ error: "DB 오류" });
    if (results.length > 0)
      return res.status(409).json({ error: "ID 중복" });

    const insertQuery =
      "INSERT INTO users (user_id, password, nickname) VALUES (?, ?, ?)";
    db.query(insertQuery, [user_id, password, nickname], (err2) => {
      if (err2) return res.status(500).json({ error: "회원가입 실패" });
      res.json({ success: true });
    });
  });
});

// 로그인
router.post("/login", (req, res) => {
  const { user_id, password } = req.body;

  const query = "SELECT * FROM users WHERE user_id = ? AND password = ?";
  db.query(query, [user_id, password], (err, results) => {
    if (err) {
      console.error("❌ 로그인 쿼리 실패:", err);
      return res.status(500).json({ message: "서버 오류" });
    }

    if (results.length > 0) {
      console.log("✅ 로그인 성공");
      res.status(200).json({ message: "로그인 성공" });
    } else {
      console.log("❌ 로그인 실패: 일치하는 유저 없음");
      res.status(401).json({ message: "아이디 또는 비밀번호가 올바르지 않습니다." });
    }
  });
});

module.exports = router;