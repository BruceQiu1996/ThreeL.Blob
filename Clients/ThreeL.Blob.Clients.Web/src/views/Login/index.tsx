import { ChangeEvent, MouseEventHandler, useEffect, useState } from 'react'
import { Space, Input, Button, message } from 'antd'
import initLoginBg from '@/views/Login/init.ts'
import styles from './login.module.scss'
import './login.less'
import { loginAPI } from "@/request/api.ts"
import { setJwtAuthToken } from "@/request/index"

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

    const OnLogin: MouseEventHandler<HTMLElement> = async () => {
        if (userName === "") {
            message.error("请输入用户名");
            return;
        }
        if (passWord === "") {
            message.error("请输入密码");
            return;
        }
        var resp = await loginAPI({ username: userName, password: passWord, origin: "web" })
        if (resp.role !== "Admin" && resp.role !== "SuperAdmin") {
            message.error("你没有权限登录后台管理系统");
            return;
        }

        localStorage.setItem("token", resp.accessToken);
        localStorage.setItem("userName", resp.userName);
        localStorage.setItem("role", resp.role);
        setJwtAuthToken(resp.accessToken);
        window.location.replace("/");
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
                        <Input.Password placeholder="密码" onChange={passWordChange} />
                        <Button type="primary" className='.loginBtn' block style={{ height: '38px', fontSize: '18px' }} onClick={OnLogin}>登录</Button>
                    </Space>
                </div>
            </div>
        </div>
    )
}
export default View