import { Route, Routes } from "react-router-dom";
import "./App.css";
import Home from "./pages/Home";
import Game from "./pages/Game";
import GameOver from "./pages/GameOver";

function App() {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/game" element={<Game />} />
      <Route path="/game-over" element={<GameOver />} />
    </Routes>
  );
}

export default App;
