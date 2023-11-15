import request from "./index";

//登录
export const loginAPI = (params: LoginRequestDto): Promise<any> => request.post("api/user/login",params);