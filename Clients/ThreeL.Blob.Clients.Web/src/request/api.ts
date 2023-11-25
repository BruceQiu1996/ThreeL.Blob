import { post, get } from "./index";

//登录
export const loginAPI = (params: LoginRequestDto): Promise<LoginResponseDto> => post<LoginResponseDto>("api/admin/login", params);
//查询用户
export const queryUsersAPI = (page: Number): Promise<QueryUsersResponseDto> => get<QueryUsersResponseDto>(`api/admin/users?page=${page}`);