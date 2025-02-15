import './App.css'
import { BrowserRouter, Routes, Route } from 'react-router'
import Index from './index/Index'
import Writer from './writer/Writer'
import Reader from './reader/Reader'
import History from './history/History'

function App() {
  return (
    <>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Index />} />
          <Route path="/writer" element={<Writer />} />
          <Route path="/reader" element={<Reader />} />
          <Route  path="/history" element={<History />}/>
        </Routes>
      </BrowserRouter>
    </>
  )
}

export default App
