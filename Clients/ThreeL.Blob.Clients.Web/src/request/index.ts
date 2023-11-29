import axios, { AxiosError } from "axios"
import { history } from '@/router/history'
import { message } from "antd"

// 鍒涘缓axios瀹炰緥
const instance = axios.create({
    // 鍩烘湰璇锋眰璺緞鐨勬娊鍙�
    baseURL: "http://127.0.0.1:5824",
    timeout: 20000,
})

export const setJwtAuthToken = (token: string) => {
    if (token) {
        // token瀛樺湪璁剧疆header,鍥犱负鍚庣画姣忎釜璇锋眰閮介渶瑕�
        instance.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    } else {
        // 娌℃湁token灏辩Щ闄�
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

export function put<T>(url: string, data: any): Promise<T> {
    return instance.put(url, data)
}

// 璇锋眰鎷︽埅鍣�
instance.interceptors.request.use(data => {
    return data
}, err => {
    return Promise.reject(err)
});

// 鍝嶅簲鎷︽埅鍣�
instance.interceptors.response.use(res => {
    return res.data
}, error => {
    if (error === undefined || error.code === 'ECONNABORTED') {
        message.warning('鏈嶅姟璇锋眰瓒呮椂')
        return Promise.reject(error)
    }
    if (error.response === undefined) {
        message.error('杩滅▼鏈嶅姟鍣ㄦ湭鍝嶅簲')
        return Promise.reject(error)
    }

    if (error.response.status === 500) {
        message.error('鏈嶅姟鍣ㄥ嚭鐜伴敊璇�')
        return Promise.reject(error)
    }

    if (error.response.status === 401) {
        message.error('鐧诲綍鍑瘉宸茶繃鏈燂紝璇烽噸鏂扮櫥褰�')
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