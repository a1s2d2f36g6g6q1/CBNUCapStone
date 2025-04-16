const express = require("express");
const router = express.Router();
const db = require("../config/game_db");

// 특정 유저의 친구 목록 불러오기
router.get("/:userId", (req, res) => {
  const userId = req.params.userId;
  const sql = "SELECT friend_id, friend_nickname, friend_profile_url FROM friends WHERE user_id = ?";
  db.query(sql, [userId], (err, result) => {
    if (err) {
      console.error("DB Error:", err);
      return res.status(500).send("DB error");
    }
    res.json(result);
  });
});

module.exports = router;
