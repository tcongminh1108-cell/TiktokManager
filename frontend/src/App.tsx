import { RouterProvider } from 'react-router-dom'
import { router } from './app/router'
import Providers from './app/Providers'

export default function App() {
  return (
    <Providers>
      <RouterProvider router={router} />
    </Providers>
  )
}
