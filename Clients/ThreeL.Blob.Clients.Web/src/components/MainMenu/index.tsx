import React, { useState } from 'react';
import {
    UserOutlined,
    FileOutlined,
    UploadOutlined,
    DownloadOutlined
} from '@ant-design/icons';
import type { MenuProps } from 'antd';
import { Menu } from 'antd';
import { useNavigate, useLocation } from 'react-router-dom';

type MenuItem = Required<MenuProps>['items'][number];
const menuItems: MenuItem[] = [
    {
        label: '用户管理',
        key: '/usersManage',
        icon: <UserOutlined />
    },
    {
        label: '文件管理',
        key: '/filesManage',
        icon: <FileOutlined />,
        children: [
            {
                label: '上传管理',
                key: '/filesManage/upload',
                icon: <UploadOutlined />
            },
            {
                label: '下载管理',
                key: '/filesManage/download',
                icon: <DownloadOutlined />
            },
        ]
    }]

const Component: React.FC = () => {
    const navigate = useNavigate();
    const MenuClick = (e: { key: string }) => {
        console.log(e.key);
        navigate(e.key);
    };

    const currentRoute = useLocation();
    const [openKeys, setOpenKeys] = useState(['']);
    const OpenChange = (keys: string[]) => {
        setOpenKeys([keys[keys.length - 1]])
    }
    return (
        <Menu theme="dark" defaultSelectedKeys={[currentRoute.pathname]} 
            selectedKeys={[currentRoute.pathname]}
            mode="inline"
            openKeys={openKeys}
            items={menuItems} onClick={MenuClick} onOpenChange={OpenChange} />
    );
};

export default Component;