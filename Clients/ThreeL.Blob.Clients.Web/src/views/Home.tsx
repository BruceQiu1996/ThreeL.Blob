import React, { useState } from 'react';
import { Breadcrumb, Layout, theme } from 'antd';
import { Outlet } from 'react-router-dom';
import MainMenu from '@/components/MainMenu';

const { Header, Content, Footer, Sider } = Layout;


const View: React.FC = () => {
    const [collapsed, setCollapsed] = useState(false);
    const {
        token: { colorBgContainer },
    } = theme.useToken();
    return (
        <Layout style={{ minHeight: '100vh' }}>
            <Sider collapsible collapsed={collapsed} onCollapse={(value) => setCollapsed(value)}>
                <div className="logo-vertical" >
                </div>
                <MainMenu />
            </Sider>
            <Layout>
                <Header style={{ padding: 0, background: colorBgContainer, paddingLeft: '16px' }} >
                    <Breadcrumb style={{ margin: '16px 0' }}>
                        <Breadcrumb.Item>User</Breadcrumb.Item>
                        <Breadcrumb.Item>Bill</Breadcrumb.Item>
                    </Breadcrumb>
                </Header>
                <Content style={{ margin: '5px 5px 0', background: colorBgContainer }}>
                    <Outlet />
                </Content>
                <Footer style={{ textAlign: 'center', padding: '0px', lineHeight: '48px' }}>Ant Design ©2023 Created by Ant UED</Footer>
            </Layout>
        </Layout>
    );
};

export default View;