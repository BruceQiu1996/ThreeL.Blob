import React, { useEffect, useState } from 'react';
import { Button, Row, Col, Table, Space } from 'antd';
import type { ColumnsType, TablePaginationConfig } from 'antd/es/table';
interface DataType {
    name: {
        first: string;
        last: string;
    };
    gender: string;
    email: string;
    login: {
        uuid: string;
    };
}
const columns: ColumnsType<DataType> =  [
    {
        title: '用户编号',
        dataIndex: 'name',
        sorter: true,
        render: (name) => `${name.first} ${name.last}`,
        width: '10%',
    },
    {
        title: '用户名',
        dataIndex: 'gender',
        filters: [
            { text: 'Male', value: 'male' },
            { text: 'Female', value: 'female' },
        ],
        width: '20%',
    },
    {
        title: '创建日期',
        dataIndex: 'email',
    },
    {
        title: '最近登录时间',
        dataIndex: 'email1',
    },
    {
        title: '最近登录时间',
        dataIndex: 'email2',
    },
    {
        title: '是否删除',
        dataIndex: 'email3',
    },
    {
        title: '操作',
        dataIndex: 'operation',
        render: () => <a>Delete</a>,
    }
];

const UserManagePage: React.FC = () => {
    const [data, setData] = useState<DataType[]>();
    return (
        <>
            <Row gutter={[5,10]} style={{marginTop:'10px'}}>
                <Col flex="auto"></Col>
                <Col flex="200px">
                    <Space>
                        <Button type="primary">新建用户</Button>
                        <Button danger>禁用用户</Button>
                    </Space>
                </Col>
                <Col span={24}>
                    <Table columns={columns}
                        rowKey={(record) => record.login.uuid}
                        dataSource={data}
                    />
                </Col>
            </Row>
        </>
    );
}

export default UserManagePage;