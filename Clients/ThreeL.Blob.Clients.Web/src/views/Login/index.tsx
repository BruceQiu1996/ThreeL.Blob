import { ChangeEvent, useEffect, useState } from 'react'
import { Space, Input, Button } from 'antd'
import initLoginBg from '@/views/Login/init.ts'
import styles from './login.module.scss'
import './login.less'
import { loginAPI } from "@/request/api.ts"

const View: React.FC = () => {
    useEffect(() => {
        initLoginBg()
        window.onresize = function () { initLoginBg() }
    }, []);

    const [userName, setUserName] = useState("");
    const userNameChange = (event: ChangeEvent<HTMLInputElement>) => {
        setUserName(event.target.value);
    }

    const [passWord, setPassWord] = useState("");
    const passWordChange = (event: ChangeEvent<HTMLInputElement>) => {
        setPassWord(event.target.value);
    }

    const OnLogin: Function = async () => {
        var resp = await loginAPI({ username: userName, password: passWord })
        console.log(resp);
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
                        <Input.Password placeholder="密码" onChange={passWordChange}/>
                        <Button type="primary" className='.loginBtn' block style={{ height: '38px', fontSize: '18px' }} onClick={OnLogin}>登录</Button>
                    </Space>
                </div>
            </div>
        </div>
    )
}
export default View