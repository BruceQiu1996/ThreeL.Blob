import axios, { AxiosError } from "axios"
import {history} from '@/router/history'
import { message } from "antd"

// 创建axios实例
const instance = axios.create({
    // 基本请求路径的抽取
    baseURL: "http://127.0.0.1:5824",
    timeout: 20000,
})

export const setJwtAuthToken = (token: string) => {
    if (token) {
        // token存在设置header,因为后续每个请求都需要
        instance.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    } else {
        // 没有token就移除
        delete instance.defaults.headers.common['Authorization'];
    }
}

setJwtAuthToken(localStorage.getItem('token')!);

export function post<T>(url: string, data: any): Promise<T> {
    return instance.post(url, data)
}

export function get<T>(url: string): Promise<T> {
    return instance.get(url)
}

// 请求拦截器
instance.interceptors.request.use(data => {
    return data
}, err => {
    return Promise.reject(err)
});

// 响应拦截器
instance.interceptors.response.use(res => {
    return res.data
}, error => {
    if (error === undefined || error.code === 'ECONNABORTED') {
        message.warning('服务请求超时')
        return Promise.reject(error)
    }
    if (error.response === undefined) {
        message.error('远程服务器未响应')
        return Promise.reject(error)
    }

    if (error.response.status === 500) {
        message.error('服务器出现错误')
        return Promise.reject(error)
    }

    if (error.response.status === 401) {
        message.error('登录凭证已过期，请重新登录')
        localStorage.removeItem('token')
        setJwtAuthToken('')
        history.push('/login')
        return Promise.reject(error)
    }

    const data = error.response.data;
    message.warning(`${data}`)
    // throw error
    // return error
    return Promise.reject(error)
})