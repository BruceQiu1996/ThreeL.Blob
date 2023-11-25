import React, { useEffect, useState } from 'react';
import { Button, Row, Col, Table, Space, Switch } from 'antd';
import type { ColumnsType, TablePaginationConfig } from 'antd/es/table';
import { queryUsersAPI } from "@/request/api.ts"
const columns: ColumnsType<UserBriefResponseDto> = [
    {
        title: '用户编号',
        dataIndex: 'id',
    },
    {
        title: '用户名',
        dataIndex: 'userName',
    },
    {
        title: '创建日期',
        dataIndex: 'createTime',
        render: (text: string) => <>{new Date(text).toLocaleString()}</>,
    },
    {
        title: '最后登录时间',
        dataIndex: 'lastLoginTime',
        render: (text: string) => text == null ? '-' : <>{new Date(text).toLocaleString()}</>,
    },
    {
        title: '是否启用',
        dataIndex: 'isDeleted',
        render: (data: boolean) => <Switch checked={!data} />
    },
    {
        title: '操作',
        dataIndex: 'operation',
        render: () => <a>Delete</a>,
    }
];

const UserManagePage: React.FC = () => {
    const [data, setData] = useState<UserBriefResponseDto[]>();
    const [totalDataCounts, setTotalDataCounts] = useState<number>();
    useEffect(() => {
        const init = async () => {
            var resp = await queryUsersAPI(0);
            setData(resp.users);
            setTotalDataCounts(resp.count);
        }

        init();
    }, []);
    return (
        <>
            <Row gutter={[5, 10]} style={{ marginTop: '10px' }}>
                <Col flex="auto"></Col>
                <Col flex="200px">
                    <Space>
                        <Button type="primary">新建用户</Button>
                        <Button danger>禁用用户</Button>
                    </Space>
                </Col>
                <Col span={24}>
                    <Table columns={columns}
                        rowKey={(record) => record.id}
                        dataSource={data}
                        pagination={{ pageSize: 10, total: totalDataCounts }}
                    />
                </Col>
            </Row>
        </>
    );
}

export default UserManagePage;