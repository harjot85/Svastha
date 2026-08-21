import { useEffect, useState } from 'react'

const apiUrl = import.meta.env.VITE_API_URL

type Ping = {
  id: number
  message: string
  createdAt: string
}

function App() {
  const [pings, setPings] = useState<Ping[]>([])
  const [message, setMessage] = useState('')
  const [error, setError] = useState<string | null>(null)

  async function loadPings() {
    const response = await fetch(`${apiUrl}/api/ping`)
    if (!response.ok) {
      throw new Error(`GET /api/ping failed with ${response.status}`)
    }
    setPings(await response.json())
  }

  useEffect(() => {
    loadPings().catch((e: unknown) => setError(String(e)))
  }, [])

  async function send() {
    setError(null)
    try {
      const response = await fetch(`${apiUrl}/api/ping`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message }),
      })
      if (!response.ok) {
        throw new Error(`POST /api/ping failed with ${response.status}`)
      }
      setMessage('')
      await loadPings()
    } catch (e: unknown) {
      setError(String(e))
    }
  }

  return (
    <main>
      <h1>Svastha</h1>

      <input
        value={message}
        onChange={(e) => setMessage(e.target.value)}
        placeholder="Message"
      />
      <button onClick={send}>Send</button>

      {error && <p>{error}</p>}

      <ul>
        {pings.map((ping) => (
          <li key={ping.id}>
            {ping.message} — {ping.createdAt}
          </li>
        ))}
      </ul>
    </main>
  )
}

export default App
