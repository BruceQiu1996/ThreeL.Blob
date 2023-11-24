import request from "./index";

//登录
export const loginAPI = (params: LoginRequestDto): Promise<LoginResponseDto> => request.post("api/user/login",params);