import { useState} from 'react';
import { Space, Input } from 'antd';
import { useDispatch,useSelector } from "react-redux"
const NewUser: React.FC = () => {
    const dispatch = useDispatch();
    const userNameChange: React.ChangeEventHandler<HTMLInputElement> = (event: React.ChangeEvent<HTMLInputElement>) => {
        dispatch({ type: "updateCreateNewUserUserName", val: event.currentTarget.value })
    }
    const passwordChange: React.ChangeEventHandler<HTMLInputElement> = (event: React.ChangeEvent<HTMLInputElement>) => {
        dispatch({ type: "updateCreateNewUserPassword", val: event.target.value })
    }
    const confirmPassswordChange: React.ChangeEventHandler<HTMLInputElement> = (event: React.ChangeEvent<HTMLInputElement>) => {
        dispatch({ type: "updateCreateNewUserConfirmPassword", val: event.target.value })
    }

    const userName = useSelector((state: RootState) => state.createNewUserReducer.userName);
    const password = useSelector((state: RootState) => state.createNewUserReducer.password);
    const confirmPassword = useSelector((state: RootState) => state.createNewUserReducer.confirmPassword);

    return (
        <div>
            {/*登陆表单*/}
            <div>
                <Space direction="vertical" size="middle" style={{ display: 'flex' }}>
                    <Input placeholder="用户名" onChange={userNameChange} value={userName}/>
                    <Input.Password placeholder="密码" onChange={passwordChange} value={password}/>
                    <Input.Password placeholder="确认密码" onChange={confirmPassswordChange} value={confirmPassword}/>
                </Space>
            </div>
        </div>
    )
}

export default NewUser;