import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { learningSetService } from '../../api/services';
import { UpdateLearningSetDto } from '../../types';
import SetEditor from './SetEditor';

const EditSetPage = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const queryClient = useQueryClient();

    const { data: set, isLoading } = useQuery({
        queryKey: ['set', id],
        queryFn: () => learningSetService.getById(id!),
        enabled: !!id,
    });

    const updateSetMutation = useMutation({
        mutationFn: (data: UpdateLearningSetDto) => learningSetService.update(id!, data),
        onSuccess: (updatedSet) => {
            queryClient.invalidateQueries({ queryKey: ['set', id] });
            navigate(`/sets/${id}`);
        }
    });

    if (isLoading) return <div>Ładowanie danych...</div>;
    if (!set) return <div>Nie znaleziono zestawu.</div>;

    return (
        <div className="container mx-auto p-4">
            <h1 className="text-2xl font-bold mb-4">Edytuj zestaw: {set.title}</h1>
            <SetEditor
                initialData={set}
                onSubmit={(data) => updateSetMutation.mutate(data)}
                isSubmitting={updateSetMutation.isPending}
            />
        </div>
    );
};

export default EditSetPage;
