const express = require("express");
const router = express.Router();
const db = require("../config/game_db");

// 특정 유저의 친구 목록 조회
router.get("/:userId", (req, res) => {
  const userId = req.params.userId;
  const sql = `
    SELECT 
      u.id AS friend_id,
      u.nickname AS friend_nickname,
      u.profile_img AS friend_profile_img
    FROM friends f
    JOIN user u ON f.friend_id = u.id
    WHERE f.user_id = ? AND f.status = 'accepted'
  `;
  
  db.query(sql, [userId], (err, results) => {
    if (err) {
      console.error("DB Error:", err);
      return res.status(500).send("DB error");
    }
    res.json(results);
  });
});

// 친구 삭제제
router.delete("/:userId/:friendId", (req, res) => {
  const { userId, friendId } = req.params;

  const sql = `
    DELETE FROM friends 
    WHERE user_id = ? AND friend_id = ?
  `;

  db.query(sql, [userId, friendId], (err, result) => {
    if (err) {
      console.error("삭제 중 DB 오류:", err);
      return res.status(500).send("삭제 중 오류 발생");
    }

    if (result.affectedRows === 0) {
      return res.status(404).send("해당 친구 관계가 없습니다");
    }

    res.send("삭제 성공");
  });
});


module.exports = router;
