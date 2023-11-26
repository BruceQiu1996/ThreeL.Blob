import React from 'react'
import ReactDOM from 'react-dom/client'
import "reset-css"
//全局样式
import "@/assets/styles/global.scss"
import App from './App'
import { unstable_HistoryRouter as HistoryRouter } from 'react-router-dom'
import { history } from '@/router/history'
import { Provider } from 'react-redux'
import store from '@/store'

ReactDOM.createRoot(document.getElementById('root')!).render(
  <Provider store={store}>
    <HistoryRouter history={history}>
      <App></App>
    </HistoryRouter>
  </Provider>
)
