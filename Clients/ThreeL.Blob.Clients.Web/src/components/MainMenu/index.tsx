import React, { useState } from 'react';
import {
    DesktopOutlined,
    FileOutlined,
    PieChartOutlined,
    TeamOutlined,
    UserOutlined,
} from '@ant-design/icons';
import type { MenuProps } from 'antd';
import { Menu } from 'antd';
import { useNavigate, useLocation } from 'react-router-dom';

type MenuItem = Required<MenuProps>['items'][number];
const menuItems: MenuItem[] = [
    {
        label: '项目一',
        key: '/page1',
        icon: <PieChartOutlined />
    },
    {
        label: '项目二',
        key: '/page2',
        icon: <DesktopOutlined />
    },
    {
        label: '项目三',
        key: 'page3',
        icon: <UserOutlined />,
        children: [
            {
                label: '项目三.1',
                key: '/page3/page301',
                icon: <DesktopOutlined />
            },
            {
                label: '项目三.2',
                key: '/page3/page302',
                icon: <DesktopOutlined />
            },
            {
                label: '项目三.3',
                key: '/page3/page303',
                icon: <DesktopOutlined />
            },
        ]
    },
    {
        label: '项目四',
        key: 'page4',
        icon: <TeamOutlined />,
        children: [
            {
                label: '项目四.1',
                key: '/page4/page401',
                icon: <DesktopOutlined />
            },
            {
                label: '项目四.2',
                key: '/page4/page402',
                icon: <DesktopOutlined />
            },
            {
                label: '项目四.3',
                key: '/page4/page403',
                icon: <DesktopOutlined />
            },
        ]
    },
    {
        label: '项目五',
        key: '/page5',
        icon: <FileOutlined />
    },
]

const Component: React.FC = () => {
    const navigate = useNavigate();
    const MenuClick = (e: { key: string }) => {
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