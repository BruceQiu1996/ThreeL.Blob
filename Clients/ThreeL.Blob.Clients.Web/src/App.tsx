import { useRoutes } from 'react-router-dom'
import router from './router'
import { ConfigProvider } from 'antd';

const App: React.FC = () => {
  const outlet = useRoutes(router);
  return (
    <ConfigProvider
      theme={{
        token: {
          // Seed Token，影响范围大
          colorPrimary: '#00b96b',
          borderRadius: 2
        },
      }}
    >
      <div>
        {outlet}
      </div>
    </ConfigProvider>
  )
};
export default App