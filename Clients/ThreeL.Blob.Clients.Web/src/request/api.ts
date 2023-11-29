import { post, get, put } from "./index";

//登录
export const loginAPI = (params: LoginRequestDto): Promise<LoginResponseDto> => post<LoginResponseDto>("api/admin/login", params);
//查询用户
export const queryUsersAPI = (page: Number): Promise<QueryUsersResponseDto> => get<QueryUsersResponseDto>(`api/admin/users?page=${page}`);
//创建用户
export const createUsersAPI = (user: UserCreationDto): Promise<any> => post<any>("api/admin/users",user);
//创建用户
export const editUsersAPI = (user: UserEditDto): Promise<UserBriefResponseDto> => put<any>(`api/admin/users/${user.id}`,user);