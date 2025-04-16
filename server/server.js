require('dotenv').config();
const express = require('express');
const app = express();
const PORT = process.env.PORT || 4000;;
const bodyParser = require('body-parser');
// const axios = require('axios');

const cors = require('cors');
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

//테스트용 API
app.get('/', (req, res) => {
  res.send('서버 작동 중!');
});

app.listen(PORT, () => {
  console.log(`서버 실행됨: http://localhost:${PORT}`);
});
