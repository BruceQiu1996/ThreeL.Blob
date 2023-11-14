import { ChangeEvent, useEffect, useState } from 'react'
import { Space, Input, Button } from 'antd'
import initLoginBg from '@/views/Login/init.ts'
import styles from './login.module.scss'
import './login.less'

const View: React.FC = () => {
    useEffect(() => {
        initLoginBg()
        window.onresize = function () { initLoginBg() }
    }, []);

    const [userName, setUserName] = useState("");
    const userNameChange = (event: ChangeEvent<HTMLInputElement>) => {
        setUserName(event.target.value);
    }

    return (
        <div className={styles.loginPage}>
            <canvas id="canvas" style={{ display: "block" }}></canvas>
            <div className={styles.loginBox}>
                <div className={styles.title}>
                    <h1>ThreeL管理后台</h1>
                    <p>Bruce Qiu</p>
                </div>
                {/*登陆表单*/}
                <div>
                    <Space direction="vertical" size="middle" style={{ display: 'flex' }}>
                        <Input placeholder="用户名" onChange={userNameChange} />
                        <Input.Password placeholder="密码" />
                        <div className='captureBox'>
                            <Input placeholder="验证码" />
                            <div className='captureBoxImg'>
                                <img src="data:image/gif;base64,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" alt="" />
                            </div>
                        </div>
                        <Button type="primary" className='.loginBtn' block style={{ height: '38px', fontSize: '18px' }}>登录</Button>
                    </Space>
                </div>
            </div>
        </div>
    )
}
export default View