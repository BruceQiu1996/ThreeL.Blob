import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { Button, Row, Col, Table, Space, Switch, Modal, message, Input, InputNumber, Select } from 'antd';
import type { ColumnsType, TablePaginationConfig } from 'antd/es/table';
import { queryUsersAPI, createUsersAPI } from "@/request/api.ts"
import NewUser from './components/CreateNewUser';

const UserManagePage: React.FC = () => {
    const [data, setData] = useState<UserBriefResponseDto[]>();
    const [totalDataCounts, setTotalDataCounts] = useState<number>();
    const [isCreateUserModalOpen, setIsCreateUserModalOpen] = useState(false);
    const [isEditUserModalOpen, setIsEditUserModalOpen] = useState(false);
    const [currentUser, setCurrentUser] = useState<UserEditDto>({} as UserEditDto);
    useEffect(() => {
        const init = async () => {
            var resp = await queryUsersAPI(1);
            setData(resp.users);
            setTotalDataCounts(resp.count);
        }

        init();
    }, []);

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
            title: '角色',
            dataIndex: 'role',
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
            render: (_, key: UserBriefResponseDto) => data!.length >= 1 ? <a onClick={() => OpenEdit(key)}>编辑</a> : null,
        }
    ];

    const OpenEdit = (row: UserBriefResponseDto) => {
        setIsEditUserModalOpen(true);
        setCurrentUser({ id: row.id, userName: row.userName, role: row.role, size: 0 });
    }

    const userName = useSelector((state: RootState) => state.createNewUserReducer.userName);
    const password = useSelector((state: RootState) => state.createNewUserReducer.password);
    const confirmPassword = useSelector((state: RootState) => state.createNewUserReducer.confirmPassword);

    const showModal = () => {
        setIsCreateUserModalOpen(true);
    };

    const handleOk = async () => {
        if (userName === "" || password === "" || confirmPassword === "") {
            message.warning("用户名或密码不能为空");
            return;
        }
        if (password !== confirmPassword) {
            message.warning("两次输入的密码不一致");
            return;
        }

        await createUsersAPI({ userName: userName, password: password });
        setIsCreateUserModalOpen(false);
    };

    const handleCancel = () => {
        setIsCreateUserModalOpen(false);
    };

    const handleEditOk = async () => {
        if (userName === "" || password === "" || confirmPassword === "") {
            message.warning("用户名或密码不能为空");
            return;
        }
        if (password !== confirmPassword) {
            message.warning("两次输入的密码不一致");
            return;
        }

        await createUsersAPI({ userName: userName, password: password });
        setIsEditUserModalOpen(false);
    };

    const handleEditCancel = () => {
        setIsEditUserModalOpen(false);
    };


    const dispatch = useDispatch();
    const handleOpenChange = (open: boolean) => {
        dispatch({ type: "updateCreateNewUserUserName", val: '' })
        dispatch({ type: "updateCreateNewUserPassword", val: '' })
        dispatch({ type: "updateCreateNewUserConfirmPassword", val: '' })
    };

    return (
        <>
            <Row gutter={[5, 10]} style={{ marginTop: '10px' }}>
                <Col flex="auto"></Col>
                <Col flex="200px">
                    <Space>
                        <Button type="primary" onClick={showModal}>新建用户</Button>
                        <Button danger>禁用用户</Button>
                    </Space>
                </Col>
                <Col span={24}>
                    <Table columns={columns}
                        rowKey={(record) => record.id}
                        dataSource={data}
                        pagination={{
                            pageSize: 10, total: totalDataCounts, onChange: async (page, pageSize) => {
                                var resp = await queryUsersAPI(page);
                                setData(resp.users);
                                setTotalDataCounts(resp.count);
                            }
                        }
                        }
                    />
                </Col>

                <Modal title="新增用户" afterOpenChange={handleOpenChange} open={isCreateUserModalOpen} onOk={handleOk} onCancel={handleCancel}>
                    <NewUser />
                </Modal>

                <Modal title="编辑用户" open={isEditUserModalOpen} onOk={handleEditOk} onCancel={handleEditCancel}>
                    <div>
                        <div>
                            <Space direction="vertical" size="middle" style={{ display: 'flex' }}>
                                <Input placeholder="用户名" value={currentUser.userName}
                                    onChange={(event: React.ChangeEvent<HTMLInputElement>) => { setCurrentUser({ ...currentUser, userName: event.target.value }) }} />
                                <Select value={currentUser.role} style={{ width: '100% ' }}
                                    onChange={(value: string) => { setCurrentUser({ ...currentUser, role: value }) }}
                                    options={[
                                        { value: 'User', label: 'User' },
                                        { value: 'Admin', label: 'Admin' },
                                        { value: 'SuperAdmin', label: 'SuperAdmin' }
                                    ]} />
                                <Space>
                                    <InputNumber style={{ width: '100% ' }} min={1} max={1000} placeholder="存储空间大小" />
                                    <label>B（字节）</label>
                                </Space>
                            </Space>
                        </div>
                    </div>
                </Modal>
            </Row>
        </>
    );
}

export default UserManagePage;