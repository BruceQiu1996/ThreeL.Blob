import React, { useState, useEffect } from 'react';
import { Breadcrumb, Layout, theme, Row, Col, MenuProps, Dropdown } from 'antd';
import { SmileOutlined, LogoutOutlined } from '@ant-design/icons';
import { Outlet } from 'react-router-dom';
import MainMenu from '@/components/MainMenu';
import { setJwtAuthToken } from '@/request';
import { history } from '@/router/history';

const { Header, Content, Footer, Sider } = Layout;

const View: React.FC = () => {
    useEffect(() => {
        setuserName(localStorage.getItem("userName") || "");
        setrole(localStorage.getItem("role") || "");
    }, [])
    const [collapsed, setCollapsed] = useState(false);
    const [userName, setuserName] = useState<string>("");
    const [role, setrole] = useState<string>("");
    const {
        token: { colorBgContainer },
    } = theme.useToken();

    const items: MenuProps['items'] = [
        {
            key: '1',
            danger: true,
            label: '退出登录',
            onClick: () => {
                localStorage.removeItem("token");
                localStorage.removeItem("userName");
                localStorage.removeItem("role");
                setJwtAuthToken('')
                history.push('/login')
            },
            icon: <LogoutOutlined />,
        },
    ];

    return (
        <Layout style={{ minHeight: '100vh' }}>
            <Sider collapsible collapsed={collapsed} onCollapse={(value) => setCollapsed(value)}>
                <div className="logo-vertical" >
                </div>
                <MainMenu />
            </Sider>
            <Layout>
                <Header style={{ padding: 0, background: colorBgContainer, paddingLeft: '16px' }} >
                    <Row>
                        <Col flex="auto">
                            <Breadcrumb style={{ margin: '16px 0' }}>
                                <Breadcrumb.Item>User</Breadcrumb.Item>
                                <Breadcrumb.Item>Bill</Breadcrumb.Item>
                            </Breadcrumb>
                        </Col>
                        <Col flex="160px" style={{ fontSize: '16px' }}>
                            <Dropdown menu={{ items }}>
                                <a>
                                    {userName} ({role})
                                </a>
                            </Dropdown>
                        </Col>
                    </Row>
                </Header>
                <Content style={{ margin: '5px 5px 0', background: colorBgContainer }}>
                    <Outlet />
                </Content>
                <Footer style={{ textAlign: 'center', padding: '0px', lineHeight: '48px' }}>Bruce ©2023 Created by dotnet 7/Ant UED</Footer>
            </Layout>
        </Layout>
    );
};

export default View;